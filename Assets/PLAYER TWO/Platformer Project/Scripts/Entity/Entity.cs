using Assets.PLAYER_TWO.Platformer_Project.Scripts.Entity.Interfaccs;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.Games;
using UnityEngine;
using UnityEngine.Splines;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.Entity
{
    /// <summary>继承了unity基类的实体基类</summary>
    public abstract class EntityBase : MonoBehaviour
    {
        protected Collider[] m_penetrationBuffer = new Collider[32]; // 碰撞渗透检测缓冲区，用于存储穿透的碰撞体
        protected Collider[] m_contactBuffer = new Collider[10]; // 碰撞检测缓冲区，用于存储接触的碰撞体

        public float accelerationMultiplier { get; set; } = 1f; // 加速度倍率
        public float gravityMultiplier { get; set; } = 1f; // 重力倍率
        public float topSpeedMultiplier { get; set; } = 1f; // 最高速度倍率
        public float turningDragMultiplier { get; set; } = 1f; // 转向阻力倍率
        public float decelerationMultiplier { get; set; } = 1f; // 减速度倍率

        public EntityEvents entityEvents;

        /// <summary>实体三维空间下的速度</summary>
        public Vector3 Velocity { get; set; } // 当前速度

        /// <summary>实体的水平速度（XoZ平面），</summary>
        public Vector3 LateralVelocity
        {
            get { return new Vector3(Velocity.x, 0, Velocity.z); }
            set { Velocity = new Vector3(value.x, Velocity.y, value.z); }
        }

        /// <summary>实体的垂直方向速度</summary>
        public Vector3 VerticalVelocity
        {
            get { return new Vector3(0, Velocity.y, 0); }
            set { Velocity = new Vector3(Velocity.x, value.y, Velocity.z); }
        }

        //忽略碰撞器缩放的实体位置
        public Vector3 unsizedPosition 
            => position - transform.up * height * 0.5f + transform.up * originalHeight * 0.5f;

        protected readonly float m_groundOffset = 0.1f; // 地面检测偏移
        protected readonly float m_slopingGroundAngle = 20f;         // 斜坡角度阈值，判断是否处于斜坡

        public bool isGrounded { get; protected set; } = true;

        public SplineContainer rails { get; protected set; } // 当前轨道

        public bool onRails { get; set;}            // 是否处于轨道上

        public CharacterController controller { get; protected set; } // 角色控制器组件

        public float originalHeight { get; protected set; } //  初始碰撞器高度

        public float lastGroundTime { get; protected set; } // 上一次处于地面的时间

        public float groundAngle { get; protected set; } // 当前地面角度

        public Vector3 groundNormal { get; protected set; } // 当前地面法线

        protected CapsuleCollider m_collider;  // 自定义胶囊体

        protected Rigidbody m_rigidbody; // 刚体组件

        public float positionDelta { get; protected set; }   // 当前位置和上一帧位置的距离
         
        public Vector3 localSlopeDirection { get; protected set; } // 当前地面的局部斜坡方向

        public RaycastHit groundHit; // 当前地面检测的碰撞信息

        public float height => controller.height; // 碰撞器当前高度

        public float radius => controller.radius; // 碰撞器半径

        public Vector3 center => controller.center; // 碰撞器中心点

        public Vector3 position => transform.position + center; // 实体当前位置

        public Vector3 stepPosition => position - transform.up * (height * 0.5f - controller.stepOffset); 

        public Vector3 lastPosition { get; set; }   // 上一帧的位置 

        protected BoxCollider m_penetratorCollider;                    // 用于检测碰撞渗透的盒状碰撞器


        public virtual void ApplyDamage(int damage, Vector3 origin) { }

        /// <summary>
        /// 判断实体是否在斜坡上
        /// </summary>
        /// <returns></returns>
        public virtual bool OnSlopingGround()
        {
            // 如果实体接触地面并且地面角度大于斜坡的角度阈值
            if (isGrounded && groundAngle > m_slopingGroundAngle)
            {
                // 从当前实体位置沿着下方发射射线进行检测
                if (Physics.Raycast(transform.position, -transform.up, out var hit, height * 2f,
                                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                {
                    // 如果射线命中的法线与世界上方向（Vector3.up）之间的夹角大于斜坡角度阈值，则认为是在斜坡上
                    return Vector3.Angle(hit.normal, Vector3.up) > m_slopingGroundAngle;
                }
                else
                    // 如果射线检测没有命中地面，认为是在斜坡上
                    return true;
            }
            return false;
        }

        public virtual void ResizeCollider(float height)
        {
            // 计算新的高度和当前高度的差值
            var delta = height - this.height;

            // 修改角色控制器的高度
            controller.height = height;

            // 调整角色控制器的中心位置，使其根据高度变化自动平移
            controller.center += Vector3.up * delta * 0.5f;
        }

        public virtual bool IsPointUnderStep(Vector3 point) => stepPosition.y > point.y;

        /// <summary>
        /// 球形检测（无返回检测信息，检测是否与其他物体发生碰撞）
        /// </summary>
        public virtual bool SphereCast(Vector3 direction, float distace,int layer = Physics.DefaultRaycastLayers,
                                       QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
        {
            // 调用带有返回检测信息的方法，并忽略返回的hit信息
            return SphereCast(direction, distace, out _, layer, queryTriggerInteraction);
        }

        /// <summary>
        /// 球形检测（返回检测信息，检测是否与其他物体发生碰撞）
        /// </summary>
        public virtual bool SphereCast(Vector3 direction, float distace, out RaycastHit hit,
                                       int layer = Physics.DefaultRaycastLayers,
                                       QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
        {
            // 计算球形检测的有效距离，确保球星的检测范围符合预期
            var castDistance = Mathf.Abs(distace - radius);
            // 使用物理引擎进行球形碰撞检测
            return Physics.SphereCast(position, radius, direction, out hit, castDistance, layer, queryTriggerInteraction);
        }

        public virtual bool CapsuleCast(Vector3 direction, float distance,
            int layer = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
        {
            // 调用带有返回检测信息的方法，并忽略返回的hit信息
            return CapsuleCast(direction,distance,out _, layer, queryTriggerInteraction);
        }

        /// <summary>
        /// 胶囊体检测（返回检测信息）
        /// </summary>
        public bool CapsuleCast(Vector3 direction,float distance,out RaycastHit hit,
            int layer = Physics.DefaultRaycastLayers,
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
        {
            // 计算胶囊体的起始位置
            var orgin = position - direction * radius + center;
            // 计算偏移量，调整胶囊体的上半部分，使得碰撞体的中心处于正确位置
            var offset = transform.up * (height * 0.5f - radius);
            // 计算胶囊体的顶部和底部位置
            var top = orgin + offset;
            var bottom = orgin - offset;

            // 使用物理引擎进行胶囊体碰撞检测
            return Physics.CapsuleCast(
                top,
                bottom,
                radius,
                direction,
                out hit,
                distance + radius,
                layer,
                queryTriggerInteraction); 
        }

        public virtual int OverlapEntity(Collider[] result, float skinOffset = 0)
        {
            // 计算接触偏移量（包括碰撞器的皮肤宽度和默认的接触偏移量）
            var contactOffset = skinOffset + controller.skinWidth + Physics.defaultContactOffset;
            // 计算重叠半径（包括胶囊碰撞器的半径和接触偏移量）
            var overlapsRadius = radius + contactOffset;
            // 计算碰撞器顶部和底部的偏移位置
            var offset = (height + contactOffset) * 0.5f - overlapsRadius;
            // 计算胶囊碰撞器的顶部位置(球心位置)
            var top = position + Vector3.up * offset;
            // 计算胶囊碰撞器的底部位置(球心位置)
            var bottom = position + Vector3.down * offset;
            // 使用 Physics.OverlapCapsuleNonAlloc 来检测与其他实体的重叠，返回重叠的实体数量
            return Physics.OverlapCapsuleNonAlloc(top, bottom, overlapsRadius, result);
        }
    }

    /// <summary>
    /// 泛型实体类，且泛型T必须继承Entity类
    /// </summary>
    public class Entity<T> : EntityBase where T : Entity<T>
    {
        public EntityStateManager<T> states { get; protected set; } // 状态管理器

        protected virtual void Awake()
        {
            InitializeController();
            InitializePenetratorCollider();
            InitializeStateManager();

        }

        // 初始化用于碰撞渗透检测的盒碰撞器（辅助碰撞检测）
        // 这个盒碰撞器的作用是检测角色是否与其它物体相交（“穿模”检测）
        protected virtual void InitializePenetratorCollider()
        {
            // 计算 XZ 平面的尺寸（直径 = 半径 * 2，减去 skinWidth 避免干扰）
            var xzSize = radius * 2f - controller.skinWidth;

            // 动态添加 BoxCollider
            m_penetratorCollider = gameObject.AddComponent<BoxCollider>();

            /*
				Slope Limit：爬坡最大角度
				Step Offset：爬梯最大高度
				Skin Width：皮肤厚度
				Min Move Distance：最小移动距离
				Center、Radius、Height：角色用于检测碰撞的胶囊体中心、半径、高
		     */
            // 设置盒碰撞器尺寸：XZ 平面是计算好的直径，高度减去stepOffset
            m_penetratorCollider.size = new Vector3(xzSize, height - controller.stepOffset, xzSize);

            // 设置碰撞器中心位置：在角色中心的基础上向上偏移 stepOffset 一半
            m_penetratorCollider.center = center + Vector3.up * controller.stepOffset * 0.5f;

            // 设置为触发器模式（不产生物理碰撞，只检测重叠）
            m_penetratorCollider.isTrigger = true;
        }

        /// <summary>
        /// 初始化胶囊体
        /// </summary>
        protected virtual void InitializeCollider()
        {
            // 动态添加 CapsuleCollider
            m_collider = gameObject.AddComponent<CapsuleCollider>();

            // 高度与 CharacterController 保持一致
            m_collider.height = controller.height;
            m_collider.radius = controller.radius;
            m_collider.center = controller.center;
            m_collider.isTrigger = true;
            m_collider.enabled = false;
        }

        /// <summary>
        /// 初始化刚体
        /// </summary>
        protected virtual void InitializeRigidbody()
        {
            m_rigidbody = gameObject.AddComponent<Rigidbody>();
            m_rigidbody.isKinematic = true;
        }

        protected virtual void InitializeController()
        {
            // 获取当前物体上的 characterController 组件
            controller = GetComponent<CharacterController>();
            // 如果没有，就动态加一个
            if (!controller)
            {
                controller = gameObject.AddComponent<CharacterController>();
            }
            // skinWidth 表示碰撞器表面到实际碰撞检测边界的距离（防止卡住用的小偏移）
            controller.skinWidth = 0.005f;
            // minMoveDistance 为最小移动距离（设为 0 表示即使移动非常小也会被检测到）
            controller.minMoveDistance = 0;
            // 记录角色控制器的初始高度（用于后续复位或高度调整）
            originalHeight = controller.height;
        }
        protected virtual void InitializeStateManager() => states = GetComponent<EntityStateManager<T>>();

        /// <summary>
        /// Entity底层加速方法
        /// </summary>
        /// <param name="direction">方向</param>
        /// <param name="turningDrag">转向阻尼</param>
        /// <param name="acceleration">加速度</param>
        /// <param name="topSpeed">最大速度</param>
        public virtual void Accelerate(Vector3 direction, float turningDrag, float acceleration, float topSpeed)
        {
            // 判断方向是否有效（不为零向量）
            if (direction.sqrMagnitude > 0)
            {
                // 计算当前速度在目标方向上的投影速度（标量）
                var speed = Vector3.Dot(direction, LateralVelocity);
                // 计算当前速度在目标方向上的向量部分
                var velocity = direction * speed;
                // 计算当前速度中垂直于目标方向的部分（转向速度）
                var turningVelocity = LateralVelocity - velocity;
                // 计算转向阻力对应的速度变化量（根据转向阻力系数和时间增量）
                var turningDelta = turningDrag * turningDragMultiplier * Time.deltaTime;
                // 计算最大允许速度（考虑速度倍率）
                var targetTopSpeed = topSpeed * topSpeedMultiplier;

                // 如果当前速度未达最大速度，或当前速度与目标方向相反，则加速
                if (LateralVelocity.magnitude < targetTopSpeed || speed < 0)
                {
                    // 增加速度，受加速度倍率和时间影响
                    speed += acceleration * accelerationMultiplier * Time.deltaTime;
                    // 限制速度在[-最大速度, 最大速度]范围内
                    speed = Mathf.Clamp(speed, -targetTopSpeed, targetTopSpeed);
                }

                // 重新计算目标方向速度向量
                velocity = direction * speed;
                // 将转向速度平滑减小到0，实现自然转向过渡
                turningVelocity = Vector3.MoveTowards(turningVelocity, Vector3.zero, turningDelta);
                // 更新横向速度为目标方向速度与转向速度之和
                LateralVelocity = velocity + turningVelocity;
            }
        }

        /// <summary>
        /// 平滑减速，速度逐渐趋于0(水平速度减速)
        /// </summary>
        /// <param name="deceleration">摩擦力</param>
        public virtual void Decelerate(float deceleration)
        {
            // 计算本真的减速度（decelerationMultiplier 可用于调节全局减速效果）
            var delta = deceleration * decelerationMultiplier * Time.deltaTime;
            // 将lateralVelocity(水平速度)逐渐插值到Vector3.zero(速度为0)
            // 第三个参数是本帧允许的最大速度变化量
            LateralVelocity = Vector3.MoveTowards(LateralVelocity, Vector3.zero, delta);
        }

        /// <summary>
        /// 让一个角色按一定旋转苏速度朝向某个方向
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="degreesPerSecond"></param>
        public virtual void FaceDirection(Vector3 direction, float degreesPerSecond)
        {
            if(direction != Vector3.zero)
            {
                var rotation = transform.rotation;
                var rotationDelta = degreesPerSecond * Time.deltaTime;
                var targrt = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(rotation, targrt, rotationDelta);
            }
        }

        public virtual void FaceDirection(Vector3 direction)
        {
            // 如果方向向量有效（不是零向量）
            if(direction.sqrMagnitude > 0)
            {
                // 生成一个面向 direction 方向的旋转（保持世界Y轴为上）
                var rotation = Quaternion.LookRotation(direction, Vector3.up);

                transform.rotation = rotation;
            }
        }

        public virtual void Gravity(float gravity)
        {
            // 如果没有接触地面
            if (!isGrounded)
            {
                // 给垂直速度叠加重力向量
                // gravityMultiplier 用于调整重力强度
                VerticalVelocity += Vector3.down * gravity * gravityMultiplier * Time.deltaTime;
            }
        }

        /// <summary>
        /// 判断实体碰撞器是否能够放入指定位置(是否与其他碰撞体重叠)
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public virtual bool FitsIntoPosition(Vector3 position)
        {
            var radius = controller.radius - controller.skinWidth;
            var offset = height * 0.5f - radius;
            var top = position + Vector3.up * offset;
            var bottom = position - Vector3.up * offset;

            return Physics.CheckCapsule(top, bottom, radius, Physics.DefaultRaycastLayers,
                                         QueryTriggerInteraction.Ignore);
        }

        protected virtual bool EvaluateLanding(RaycastHit hit)
        {
            return IsPointUnderStep(hit.point) && Vector3.Angle(hit.normal,Vector3.up) < controller.slopeLimit;
        }

        protected virtual void EnterGround(RaycastHit hit)
        {
            if(!isGrounded)
            {
                groundHit = hit;
                isGrounded = true;
                entityEvents.OnGroundEnter?.Invoke();
            }
        }

        protected virtual void ExitGround()
        {
            if(isGrounded)
            {
                isGrounded = false;
                transform.parent = null;
                lastGroundTime =Time.time;
                VerticalVelocity = Vector3.Max(VerticalVelocity,Vector3.zero);
                entityEvents.OnGroundExit?.Invoke();
            }
        }

        // 处理碰撞体的穿透修正,碰到别的东西给反推回来
        protected virtual void HandlePenetration()
        {
            // 获取盒碰撞器在 X/Z 平面的半宽
            var xzSize = m_penetratorCollider.size.x * 0.5f;
            // 获取盒碰撞器在 Y 方向的半高（考虑 stepOffset 的一半）
            var ySize = (height - controller.stepOffset * 0.5f) * 0.5f;
            // 盒碰撞器的中心点（向上偏移 stepOffset 一半）
            var origin = position + Vector3.up * controller.stepOffset * 0.5f;
            // 盒碰撞器的半尺寸
            var halfExtents = new Vector3(xzSize, ySize, xzSize);

            // 检测所有与盒碰撞器重叠的物体，忽略触发器
            var overlaps = Physics.OverlapBoxNonAlloc(
                origin,
                halfExtents,
                m_penetrationBuffer,
                Quaternion.identity,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore
            );

            // 遍历所有重叠的碰撞体
            for (int i = 0; i < overlaps; i++)
            {
                // 排除触发器、自身，并且只在静止或与平台接触时处理
                if (!m_penetrationBuffer[i].isTrigger && m_penetrationBuffer[i].transform != transform &&
                    (LateralVelocity.sqrMagnitude == 0 || m_penetrationBuffer[i].CompareTag(GameTags.Platform)))
                {
                    // ComputePenetration 计算两个碰撞体的分离向量和距离
                    if (Physics.ComputePenetration(
                        m_penetratorCollider, position, Quaternion.identity,
                        m_penetrationBuffer[i], m_penetrationBuffer[i].transform.position,
                        m_penetrationBuffer[i].transform.rotation,
                        out var direction, out float distance))
                    {
                        // 只保留水平方向的推离（避免角色被垂直方向顶飞）
                        var pushDirection = new Vector3(direction.x, 0, direction.z).normalized;

                        // 直接把角色位置沿推离方向移动（修正穿透）
                        transform.position += pushDirection * distance;
                    }
                }
            }
        }

        public virtual void UpdateGround(RaycastHit hit)
        {
            if (isGrounded)
            {
                groundHit = hit;
                groundNormal = groundHit.normal;
                groundAngle = Vector3.Angle(Vector3.up, groundHit.normal);
                localSlopeDirection = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
                transform.parent = hit.collider.CompareTag(GameTags.Platform) ? hit.transform : null;
            }
        }

        // 根据斜坡角度调整速度（模拟上坡减速、下坡加速）
        public virtual void SlopeFactor(float upwardForce, float downwardForce)
        {
            // 必须接触地面，且当前地面是斜坡，才进行处理
            if (!isGrounded || !OnSlopingGround()) return;

            // factor 表示斜坡倾斜程度（法线越接近 Vector3.up，factor 越接近 1）
            var factor = Vector3.Dot(Vector3.up, groundNormal);
            // 检查当前水平速度是否沿着斜坡向下
            var downwards = Vector3.Dot(localSlopeDirection, LateralVelocity) > 0;
            // 根据方向选择上坡力或下坡力
            var multiplier = downwards ? downwardForce : upwardForce;
            // 计算本帧速度调整量
            var delta = factor * multiplier * Time.deltaTime;

            // 在 localSlopeDirection 方向上增加或减少速度
            LateralVelocity += localSlopeDirection * delta;
        }

        public virtual void SnapToGround(float force)
        {
            if(isGrounded && (VerticalVelocity.y <= 0))
            {
                VerticalVelocity = Vector3.down * force;
            }
        }

        protected virtual void HandleController()
        {
            if (controller.enabled)
            {
                controller.Move(Velocity * Time.deltaTime);
                return;
            }
            transform.position += Velocity * Time.deltaTime;
        }

        protected virtual void HandleStates() => states.Step();

        /// <summary>
        /// 处理角色与地面的检测与相关逻辑
        /// </summary>
        protected virtual void HandleGround()
        {
            if(onRails) return; // 角色在轨道模式下，不做地面检测

            // 距离计算： 角色半高 + 地面检测的额外偏移量
            var distence = (height * 0.5f) + m_groundOffset;

            // 向下发射球体射线检测地面：并且角色的垂直速度 <= 0 (下落或禁止状态)
            if(SphereCast(Vector3.down, distence, out var hit) && VerticalVelocity.y <= 0)
            {
                // 如果之前不在地面状态
                if(!isGrounded)
                {
                    // 判断是否满足落地条件
                    if (EvaluateLanding(hit))
                    {
                        // 进入落地逻辑
                        EnterGround(hit);
                    }
                    else
                    {
                        // 否则是接近高台边缘的情况
                        HandleHighLedge(hit);
                    }
                } // 已在地面状态 
                else if(IsPointUnderStep(hit.point))
                {
                    // 更新地面信息
                    UpdateGround(hit);

                    // 如果地面坡度角度超过允许值
                    if (Vector3.Angle(hit.normal, Vector3.up) >= controller.slopeLimit)
                    {
                        // 处理坡度限制（可能是滑落或停止移动）
                        HandleSlopeLimit(hit);
                    }
                }
                else
                {
                    // 不在台阶下方，则可能是悬空或高台边缘
                    HandleHighLedge(hit);
                }
            }
            else
            {
               ExitGround(); // 退出地面逻辑
            }
        }

        /// <summary>
        /// 处理接触事件
        /// </summary>
        /// <param name="other"></param>
        protected virtual void OnContact(Collider other)
        {
            if (other)
            {
                states.OnContact(other);
            }
        }

        // 处理坡度限制（空实现，子类重写）
        protected virtual void HandleSlopeLimit(RaycastHit hit) { }

        // 处理高地（空实现，子类重写）
        protected virtual void HandleHighLedge(RaycastHit hit) { }

        protected virtual void OnUpdate() { }

        /// <summary>
        /// 检测与其他物体的接触
        /// </summary>
        protected virtual void HandleContacts()
        {
            // 检测与其他实体的重叠，并把结果存入 m_contactBuffer
            var overlaps = OverlapEntity(m_contactBuffer);

            // 遍历所有重叠碰撞体
            for (int i = 0;i < overlaps; i++)
            {
                // 调用触发器碰撞体和自身碰撞体
                if (!m_contactBuffer[i].isTrigger && m_contactBuffer[i].transform != transform)
                {
                    // 调用本对象的接触回调
                    OnContact(m_contactBuffer[i]);

                    // 获取该物体上的所有实现 IEntityContact 接口的组件
                    var listeners = m_contactBuffer[i].GetComponents<IEntityContact>();
                    
                    // 依次调用对方的接触回调(实现双向交互)
                    foreach(var contact in listeners)
                    {
                        contact.OnEntityContact((T)this);
                    }

                    // 如果接触物体的地面高于角色的顶部(即碰撞到头顶的物体)
                    if (m_contactBuffer[i].bounds.min.y > controller.bounds.max.y)
                    {
                        // 限制向上的垂直速度(防止继续向上穿透)
                        VerticalVelocity = Vector3.Min(VerticalVelocity, Vector3.zero);
                    }
                }
            }
            
        }

        /// <summary>
        /// 处理轨道逻辑
        /// </summary>
        protected virtual void HandleSpline()
        {
            var distance = (height * 0.5f) + height * 0.5f;
            if(SphereCast(-transform.up,distance,out var hit) && hit.collider.CompareTag(GameTags.InteractiveRail))
            {
                if(!onRails && VerticalVelocity.y <= 0)
                {
                    EnterRail(hit.collider.GetComponent<SplineContainer>());
                }
            }
            else
            {
                ExitRail();
            }
        }

        public virtual void EnterRail(SplineContainer rails)
        {
            if(!onRails)
            {
                onRails = true; // 标记角色进入轨道状态
                // 保存当前轨道数据引用
                this.rails = rails;
                //触发进入轨道事件
                entityEvents.OnRailsEneter.Invoke();
            }
        }

        public virtual void ExitRail()
        {
            // 只有当前在轨道状态才执行
            if (onRails)
            {
                // 标记角色不在轨道模式
                onRails = false;
                // 触发退出轨道的事件
                entityEvents.OnRailsExit.Invoke();
            }
        }

        public virtual void UseCustomCollision(bool value)
        {
            controller.enabled = !value;
            if (value)
            {
                InitializeCollider();
                InitializeRigidbody();
            }
            else
            {
                Destroy(m_collider);
                Destroy(m_rigidbody);
            }
        }

        protected virtual void Update()
        {
            if (controller.enabled || m_collider!= null)
            {
                HandleStates();
                HandleController();
                HandleSpline();
                HandleGround();
                HandleContacts();
                OnUpdate();
            }
        }

        /// <summary>
        /// 计算位置变化，用于计算位移
        /// </summary>
        protected virtual void HandlePosition()
        {
            // 计算当前位置与上一帧位置的距离（位置长度）
            positionDelta = (position - lastPosition).magnitude;

            // 更新上一帧的位置
            lastPosition = position;
        }

        /// <summary>
        /// 收尾工作，位置/相机最终修正，当LateUpdate里合适
        /// </summary>
        protected virtual void LateUpdate()
        {
            if (controller.enabled)
            {
                HandlePosition();
                HandlePenetration();
            }
        }
    }
}


