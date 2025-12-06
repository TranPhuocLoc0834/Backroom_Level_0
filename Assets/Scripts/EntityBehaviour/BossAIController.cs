using UnityEngine;
using UnityEngine.AI; 

public class BossAIController : MonoBehaviour
{
    // --- Các tham chiếu và Biến Cần thiết ---
    private NavMeshAgent agent;
    private Animator animator; 
    public Transform playerTarget; 
    
    // --- Tham số AI ---
    public float detectRange = 12f;      
    public float attackRange = 3f;       
    public float roamRadius = 30f;       
    public float patrolSpeed = 1.5f;     
    public float chaseSpeed = 5.0f;      
    public float maxCooldownTime = 2f;    

    // --- Biến FSM & Chu kỳ Tấn công ---
    public enum BossState { FREE_ROAM, CHASE, ATTACK, COOLDOWN }
    public BossState currentState = BossState.FREE_ROAM;
    private float cooldownTimer;
    private Vector3 randomRoamTarget; 
    
    // Logic Chu kỳ Tấn công Cố định: Kéo (0) -> 1 Tay (1) -> 2 Tay (2)
    private readonly int[] attackSequence = { 0, 1, 2 }; 
    private int currentAttackCycleIndex = 0; 
    
    // Biến Chống Kẹt
    private float stuckTimer;             
    public float maxStuckTime = 3f;       

    // ====================================================================
    // 1. KHỞI TẠO (START)
    // ====================================================================

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); 
        
        if (agent == null || animator == null)
        {
            Debug.LogError("Thiếu component: NavMeshAgent hoặc Animator trên Boss.");
        }

        randomRoamTarget = RandomNavSphere(transform.position, roamRadius);
        agent.speed = patrolSpeed;
        
        // Log trạng thái khởi tạo
        float initialDistance = 0f;
        if (playerTarget != null)
        {
            initialDistance = Vector3.Distance(transform.position, playerTarget.position);
        }
        
        Debug.Log($"[FSM DEBUG] Khởi tạo Trạng thái: {currentState} | Khoảng cách ban đầu: {initialDistance:F2}"); 
    }

    // ====================================================================
    // 2. LOGIC AI VÀ ANIMATION CHÍNH
    // ====================================================================

    void Update()
    {
        if (playerTarget == null) return; 
        
        LogContinuousData(); // [MỚI] Gọi hàm log liên tục
        UpdateAnimator(); 
        UpdateBossAI(); 
    }

    // [MỚI] Hàm để in ra dữ liệu liên tục (khoảng cách và tốc độ)
    private void LogContinuousData()
    {
        if (agent == null || playerTarget == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        float currentSpeed = agent.velocity.magnitude;

        // In ra dữ liệu liên tục (Mỗi khung hình)
        Debug.Log($"[CONTINUOUS] Khoảng cách: {distanceToPlayer:F2} | Vận tốc thực: {currentSpeed:F2} | Trạng thái: {currentState}");
    }
    
    private void UpdateAnimator()
    {
        if (agent == null || animator == null) return;

        float desiredSpeed = agent.velocity.magnitude;
        float currentAnimatorSpeed = animator.GetFloat("Speed");
        
        float smoothedSpeed = Mathf.Lerp(
            currentAnimatorSpeed, 
            desiredSpeed, 
            Time.deltaTime * 10f 
        );

        // [LOG TỐC ĐỘ]
        //Debug.Log($"[SPEED DEBUG] Smooth Speed: {smoothedSpeed:F2} | Trạng thái: {currentState}"); 

        animator.SetFloat("Speed", smoothedSpeed); 
    }


    void UpdateBossAI()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        BossState previousState = currentState; 
        
        // --- LOGIC CHỐNG KẸT ---
        if (currentState == BossState.FREE_ROAM)
        {
            if (agent.remainingDistance > 0.5f && agent.velocity.magnitude < 0.1f)
            {
                stuckTimer += Time.deltaTime;
            }
            else
            {
                stuckTimer = 0f;
            }

            if (stuckTimer > maxStuckTime)
            {
                RotateToEscape(); 
                randomRoamTarget = RandomNavSphere(transform.position, roamRadius);
                stuckTimer = 0f; 
            }
        }
        
        // --- Quy tắc Chuyển đổi Ưu tiên Cao (KHỐI KHỞI TẠO CHASE) ---
        // Chỉ kích hoạt CHASE nếu đang ở FREE_ROAM
        if (distanceToPlayer < detectRange && currentState == BossState.FREE_ROAM) 
        {
            currentState = BossState.CHASE;
        }

        if (currentState != previousState)
        {
            // [CẬP NHẬT LOG NÀY] Xác định Tốc độ Mục Tiêu của trạng thái mới để in ra log
            float targetSpeed = 0f;
            if (currentState == BossState.CHASE) targetSpeed = chaseSpeed;
            else if (currentState == BossState.FREE_ROAM) targetSpeed = patrolSpeed;
            else if (currentState == BossState.COOLDOWN || currentState == BossState.ATTACK) targetSpeed = 0f;
            
            // In ra Tốc độ MỤC TIÊU (Target Speed) mới
            Debug.Log($"[FSM DEBUG] CHUYỂN TRẠNG THÁI: {previousState} -> {currentState} | Khoảng cách: {distanceToPlayer:F2} | Tốc độ MỤC TIÊU: {targetSpeed:F2}");
        }

        // --- Xử lý Hành động dựa trên Trạng thái hiện tại ---
        switch (currentState)
        {
            case BossState.FREE_ROAM:
                HandleFreeRoam();
                break;
            case BossState.CHASE:
                HandleChase(distanceToPlayer);
                break;
            case BossState.ATTACK:
                HandleAttack();
                break;
            case BossState.COOLDOWN:
                HandleCooldown();
                break;
        }
    }

    // ====================================================================
    // 3. XỬ LÝ TRẠNG THÁI DI CHUYỂN
    // ====================================================================

    void HandleFreeRoam()
    {
        agent.isStopped = false; 
        agent.speed = patrolSpeed;
        
        agent.SetDestination(randomRoamTarget);
        
        if (!agent.pathPending && agent.remainingDistance < 1f) 
        {
            randomRoamTarget = RandomNavSphere(transform.position, roamRadius); 
        }
    }
    
    void HandleChase(float distanceToPlayer)
{
    // Logic ưu tiên tấn công
    if (distanceToPlayer < attackRange)
    {
        currentState = BossState.ATTACK;
        return;
    }
    // Logic thoát: Nếu người chơi quá xa (1.5 lần detectRange)
    else if (distanceToPlayer > detectRange * 1.5f) 
    {
        // Log tại thời điểm thoát khỏi CHASE, thêm tốc độ mới (patrolSpeed)
        Debug.Log($"[FSM DEBUG] Thoát CHASE: Khoảng cách {distanceToPlayer:F2} > Ngưỡng {detectRange * 1.5f:F2} | Tốc độ Mới: {patrolSpeed:F2}");
        
        currentState = BossState.FREE_ROAM; // Đặt trạng thái thoát
        randomRoamTarget = RandomNavSphere(transform.position, roamRadius);
        
        // [CẬP NHẬT QUAN TRỌNG] Thoát khỏi hàm HandleChase ngay lập tức
        return; 
    }
    
    // Nếu không thoát, tiếp tục truy đuổi:
    agent.isStopped = false;
    agent.speed = chaseSpeed;
    agent.SetDestination(playerTarget.position);
}

    // ====================================================================
    // 4. XỬ LÝ TRẠNG THÁI TẤN CÔNG (KÍCH HOẠT ANIMATION THEO CHU KỲ)
    // ====================================================================

    void HandleAttack()
{
    agent.isStopped = true; 
    agent.speed = 0f;

    if (animator != null)
    {
        int attackIndexToSend = attackSequence[currentAttackCycleIndex]; 

        animator.SetInteger("AttackIndex", attackIndexToSend);
        animator.SetTrigger("IsAttacking");
        
        Debug.Log($"[ACTION] BẮT ĐẦU CHUỖI COMBO: Kích hoạt Index {attackIndexToSend}");
        
        // [CẬP NHẬT] Tăng Index để chuyển sang đòn tiếp theo sau khi kích hoạt đòn này
        currentAttackCycleIndex++;
        
        // GHI CHÚ: Logic reset Index về 0 khi đủ 3 đòn đã được chuyển sang HandleCooldown().
    }

    // [LỖI LOGIC ĐÃ SỬA] Đặt COOLDOWN (2 giây) để Boss thực hiện hết Animation
    currentState = BossState.COOLDOWN;
    cooldownTimer = maxCooldownTime;
}

    void HandleCooldown()
{
    cooldownTimer -= Time.deltaTime; 

    if (cooldownTimer <= 0)
    {
        // KHÔNG đặt agent.isStopped = false ở đây nữa.
        // agent.isStopped phải được điều chỉnh khi chuyển trạng thái.
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // ... (Logic phá vỡ chu kỳ) ...

        // Logic Chu kỳ Liên hoàn
        if (currentAttackCycleIndex >= attackSequence.Length)
        {
            // CHU KỲ HOÀN TẤT -> CHUYỂN SANG CHASE
            currentAttackCycleIndex = 0; 
            currentState = BossState.CHASE;
            
            // [QUAN TRỌNG] KÍCH HOẠT DI CHUYỂN NGAY LẬP TỨC
            HandleChase(distanceToPlayer); 
        }
        else
        {
            // Chu kỳ CHƯA hoàn tất -> Vẫn tiếp tục dừng trong ATTACK
            currentState = BossState.ATTACK;
            // Không cần làm gì khác, vì HandleAttack sẽ tự động dừng Boss
        }
    }
}
    // ====================================================================
    // 5. HÀM HỖ TRỢ & CHỐNG KẸT
    // ====================================================================

    Vector3 RandomNavSphere(Vector3 origin, float range)
    {
        Vector3 randomDirection = Random.insideUnitSphere * range;
        randomDirection += origin;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, range, NavMesh.AllAreas);
        
        return hit.position; 
    }
    
    private void RotateToEscape()
    {
        float angle = (Random.value < 0.5f) ? 45f : -45f;
        Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, angle, 0);
        
        transform.rotation = targetRotation; 
    }
}