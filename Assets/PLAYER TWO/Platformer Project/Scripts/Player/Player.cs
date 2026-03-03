using Assets.PLAYER_TWO.Platformer_Project.Scripts.Entity;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.Games;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.Misc;
using Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib.State;
using UnityEngine;

namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.PlayerLib
{
    public class Player : Entity<Player>
    {
        public PlayerEvents playerEvents; // 玩家事件（受伤，死亡，拾取物品等触发的事件）

        /// <summary> 玩家输入管理器实例  </summary>
        public PlayerInputManager inputs { get; protected set; }

        /// <summary> 玩家数值管理器实例  </summary>
        public PlayerStatsManager stats { get; protected set; }

        /// <summary> 玩家已跳跃的次数  </summary>
        public int jumpCounter { get; protected set; }

        /// <summary> 玩家已进行空中冲刺的次数  </summary>
        public int airDashCounter { get; protected set; }

        /// <summary> 上一次冲刺的时间  </summary>
        public float lastDashTime { get; protected set; }

        /// <summary> 玩家已进行空中旋转的次数  </summary>
        public int airSpinCounter { get; protected set; }

        /// <summary> 玩家是否在水中 </summary>
        public bool onWater { get; protected set; }

        /// <summary> 生命值实例 </summary>
        public Health health { get; protected set; }

        // 皮肤初始位置与旋转（用于恢复外观）
        protected Vector3 m_skinInitialPosition = Vector3.zero;
        protected Quaternion m_skinInitialRotation = Quaternion.identity;

        /// <summary> 玩家角色皮肤 </summary>
        public Transform skin;

        /// <summary>玩家是否正在持有物品</summary>
        public bool holding { get; protected set; }

        /// <summary>玩家当前攀爬的杆子</summary>
        public Pole pole { get; protected set; }

        /// <summary>玩家最后接触到的墙面法线（用于蹬墙跳等逻辑）</summary>
        public Vector3 lastWallNormal {  get; protected set; }

        /// <summary>玩家当前所在的水域碰撞器</summary>
        public Collider water { get; protected set; }

        /// <summary>玩家当前持有的可拾取物体实例</summary>
        public Pickable pickable { get; protected set; }

        /// <summary>玩家手持物品的挂点</summary>
        public Transform pickableSlot;

        // 玩家从水中出来时的微小偏移
        protected const float k_waterExitOffset = 0.35f;

        // 玩家重生点位置与旋转
        protected Vector3 m_respawnPosition;
        protected Quaternion m_respawnRotation;

        // 玩家是否存活
        public virtual bool isAlive => !health.isEmpty;

        protected override void Awake()
        {
            base.Awake();
            InitializeInputs();
            InitializeStats();
            InitializeHealth();
            InitializeTag();
            InitializeRespawn();

            // 监听落地事件，重置跳跃/空中技能次数
            entityEvents.OnGroundEnter.AddListener(() =>
            {
                ResetJumps();
                ResetAirDash();
                ResetAirSpins();
            });

            // 监听进入轨道事件，重置空中技能并进入滑轨状态
            entityEvents.OnRailsEneter.AddListener(() =>
            {
                ResetJumps();
                ResetAirDash();
                ResetAirSpins();
                StartGrind();
            });
        }

        // 初始化输入
        protected virtual void InitializeInputs() => inputs = GetComponent<PlayerInputManager>();

        // 初始化数值
        protected virtual void InitializeStats() => stats = GetComponent<PlayerStatsManager>();

        // 初始化生命
        protected virtual void InitializeHealth() => health = GetComponent<Health>();

        // 初始化标签
        protected virtual void InitializeTag() => tag = GameTags.Player;

        /// <summary>
        /// 开始轨道滑行
        /// </summary>
        public virtual void StartGrind() => states.Change<RailGrindPlayerState>();

        public virtual void Accelerate(Vector3 direction)
        {
            // 根据是否按下 Run 键，是否在地面，决定不同的转向阻尼和加速度
            var turningDrag = isGrounded && inputs.GetRun() ?
                              stats.current.runingTurningDrag : stats.current.turningDrag;
            var acceleration = isGrounded && inputs.GetRun() ?
                              stats.current.runingAcceleration : stats.current.acceleration;

            var finalAcceleration = isGrounded ? acceleration : stats.current.airAcceleration; // 空中与地面不同
            var topSpeed = inputs.GetRun() ? stats.current.runingTopSpeed : stats.current.topSpeed;

            Accelerate(direction, turningDrag, finalAcceleration, topSpeed);
        }

        /// <summary>
        /// 在指定方向上平滑移动玩家（匍匐状态的参数）
        /// </summary>
        /// <param name="direction"></param>
        public virtual void CrawlingAccelerate(Vector3 direction)
            => Accelerate(
                direction,
                stats.current.crawlingTurningSpeed,
                stats.current.crawlingAcceleration,
                stats.current.crawlinngTopSpeed);

        /// <summary>
        /// 在后空翻动作中平滑移动玩家 
        /// </summary>
        public virtual void BackflipAcceleration()
        {
            var direction = inputs.GetMovementCameraDirection();
            Accelerate(
                direction,
                stats.current.backflipTurningDrag,
                stats.current.backflipAirAcceleration,
                stats.current.backflipTopSpeed);
        }

        public virtual void Decelerate() => Decelerate(stats.current.deceleration);

        /// <summary>
        /// 平滑减速（使用摩擦力参数）
        /// </summary>
        public virtual void Friction()
        {
            if (OnSlopingGround())
            {
                Decelerate(stats.current.slopeFriction);
            }
            else
            {
                Decelerate(stats.current.friction);
            }
        }

        public virtual void AccelerateToInputDirection()
        {
            var inputDirection = inputs.GetMovementCameraDirection();
            Accelerate(inputDirection);
        }

        /// <summary>
		/// 应用标准的斜坡修正因子（在上坡 / 下坡时修改移动表现）
		/// </summary>
		public virtual void RegularSlopeFactor()
        {
            if (stats.current.applySlopeFactor)
                SlopeFactor(stats.current.slopeUpwardForce, stats.current.slopeDownwardForce);
        }

        public virtual void FaceDirectionSmooth(Vector3 direction)
                            => FaceDirection(direction, stats.current.rotationSpeed);
        /// <summary>
        /// 施加重力，使玩家下落
        /// </summary>
        public virtual void Gravity()
        {
            if (!isGrounded && VerticalVelocity.y > -stats.current.gravityTopSpeed)
            {
                var speed = VerticalVelocity.y;
                var force = VerticalVelocity.y > 0 ? stats.current.gravity : stats.current.fallGravity;
                speed -= force * gravityMultiplier * Time.deltaTime;
                // 限制最大下落速度
                speed = Mathf.Max(speed, -stats.current.gravityTopSpeed);
                VerticalVelocity = new Vector3(0, speed, 0);
            }
        }

        public virtual void SnapToGround() => SnapToGround(stats.current.snapForce);

        public override void ApplyDamage(int amount, Vector3 origin)
        {
            if (!health.isEmpty && !health.recovering)
            {
                health.Damage(amount);
                var damageDir = origin - transform.position; // 计算受击方向
                damageDir.y = 0; // 忽略垂直方向
                damageDir = damageDir.normalized;
                FaceDirection(damageDir); // 面向攻击方向

                // 受伤时向后击退
                LateralVelocity = -transform.forward * stats.current.hurtBackwardsForce;

                if (!onWater) // 如果不在水中，则会被击飞向上并进入受伤状态
                {
                    VerticalVelocity = Vector3.up * stats.current.hurtUpwardForce;
                    states.Change<HurtPlayerState>();
                }

                playerEvents.OnHurt?.Invoke();

                // 如果血量空了 -> 死亡
                if (health.isEmpty)
                {
                    Throw(); // 丢掉物品
                    playerEvents.OnDie?.Invoke();   // 触发死亡事件

                }
            }
        }

        /// <summary> 玩家是否可以站立，通过 SphereCast 检测头顶是否有障碍物 </summary>
        public virtual bool canStandUp => !SphereCast(Vector3.up, originalHeight);

        /// <summary>
        /// 后空翻
        /// </summary>
        /// <param name="force">后空翻移动力</param>
        public virtual void Backflip(float force)
        {
            if (stats.current.canBackflip && !holding)
            {
                VerticalVelocity = Vector3.up * stats.current.backflipJumpHeight; // 上跳力
                LateralVelocity = -transform.forward * force;
                states.Change<BackflipPlayerState>();
                playerEvents.OnBackflip?.Invoke();
            }
        }

        /// <summary>
        /// 拾取物体或投掷物体(根据当前状态决定是拾取还是丢出)
        /// </summary>
        public virtual void PickAndThrow()
        {
            // 玩家当前允许拾取并且输入了"拾取/放下"按键
            if(stats.current.canPickUp && inputs.GetPickAndDropDown())
            {
                if(!holding)    // 如果当前没有拿着东西
                {
                    // 在角色前方做一个 CapsuleCast(胶囊体检测)，看是否有可拾取物
                    if(CapsuleCast(transform.forward,stats.current.pickDistance,out var hit))
                    {
                        // 如果检测到的物体有 Pickable 组件，则执行拾取
                        if(hit.transform.TryGetComponent(out Pickable pickable))
                        {
                            PickUp(pickable);
                        }
                    }
                }
                else // 当前拿着物品，则执行投掷
                {
                    Throw();
                }
            }
        }

        /// <summary>
		/// 处理斜坡限制：当斜坡过陡时，推动玩家往斜坡下方滑动
		/// </summary>
		protected override void HandleSlopeLimit(RaycastHit hit)
        {
            if (onWater) return; // 如果在水中则不处理斜坡逻辑

            // 根据法线计算斜坡方向：
            // 1. hit.normal = 碰撞表面法线
            // 2. Vector3.Cross(hit.normal, Vector3.up) = 斜坡的横向向量
            // 3. Cross(hit.normal, 横向向量) = 斜坡下滑的方向
            var slopeDirection = Vector3.Cross(hit.normal, Vector3.Cross(hit.normal, Vector3.up));
            slopeDirection = slopeDirection.normalized;

            // 按照滑动力 stats.current.slideForce 推动角色沿斜坡下滑
            controller.Move(slopeDirection * stats.current.slideForce * Time.deltaTime);
        }

        /// <summary>
		/// 处理过高的台阶：当撞到高边缘时推动玩家离开
		/// </summary>
		protected override void HandleHighLedge(RaycastHit hit)
        {
            if (onWater) return;

            // 计算边缘方向 = 碰撞点 - 玩家位置
            var edgeNormal = hit.point - position;
            // 通过 Cross 计算推动方向，让玩家沿边缘推开
            var edgePushDirection = Vector3.Cross(edgeNormal, Vector3.Cross(edgeNormal, Vector3.up));

            // 施加一个推力（使用 gravity 值作为强度），让玩家远离过高的边缘
            controller.Move(edgePushDirection * stats.current.gravity * Time.deltaTime);
        }

        /// <summary>
		/// 着陆判定：调用基类的判定，并且排除 Spring（弹簧）作为有效地面
		/// </summary>
		protected override bool EvaluateLanding(RaycastHit hit)
        {
            return base.EvaluateLanding(hit) && !hit.collider.CompareTag(GameTags.Spring);
        }



        /// <summary>
        /// 拾取物品逻辑
        /// </summary>
        /// <param name="pickable"></param>
        public virtual void PickUp(Pickable pickable)
        {
            // 必须没有拿东西，并且玩家在地面上或允许空中拾取
            if (!holding && (isGrounded || stats.current.canPickUpOnAir))
            {
                holding = true; // 标记正在持有物品
                this.pickable = pickable;   // 记录被拾取的物品
                pickable.PickUp(pickableSlot);  // 把物品附着到拾取点(手，头顶等)
                pickable.onRespawn.AddListener(RemovePickable); // 监听物品的重生事件，如果物品重生就清除引用
                playerEvents.OnPickUp?.Invoke();    // 触发拾取事件
            }
        }

        /// <summary>
        /// 移除手中持有的物品(例如物品消失或重生时)
        /// </summary>
        public virtual void RemovePickable()
        {
            if (holding)
            {
                pickable = null;
                holding = false;
            }
        }

        /// <summary>
        /// 投掷物品逻辑
        /// </summary>
        public virtual void Throw()
        {
            if (holding)
            {
                // 投掷力与玩家的水平移动速度相关
                var force = LateralVelocity.magnitude * stats.current.throwVelocityMultiplier;
                pickable.Release(transform.forward, force);  //  按角色前方向丢出
                pickable = null;        //  清除物品引用
                holding = false;        // 置空持有状态
                playerEvents.OnThrow?.Invoke();     // 触发投掷事件

            }
        }

        /// <summary>
        /// 初始化重生点
        /// </summary>
        protected virtual void InitializeRespawn()
        {
            m_respawnPosition = transform.position;
            m_respawnRotation = transform.rotation;
        }

        /// <summary>
        /// 设置下次重生的位置与旋转
        /// </summary>
        public virtual void SetRespawn(Vector3 position, Quaternion rotation)
        {
            m_respawnPosition = position;
            m_respawnRotation = rotation;
        }

        public virtual void Respawn()
        {
            health.Reset(); // 重置生命
            transform.SetPositionAndRotation(m_respawnPosition, m_respawnRotation);
            states.Change<IdlePlayerState>();
        }

        /// <summary>
        /// 空中俯冲
        /// </summary>
        public virtual void AirDive()
        {
            // 必须允许空中俯冲，且不在地面上，没有拿物品，按下了空中俯冲按键
            if (stats.current.canAirDive && !isGrounded && inputs.GetAirDiveDown())
            {
                states.Change<AirDivePlayerState>(); // 切换到空中俯冲状态
                playerEvents.OnAirDive?.Invoke();
            }
        }

        /// <summary>
        /// 重置空中冲刺次数
        /// </summary>
        public virtual void ResetAirDash() => airDashCounter = 0;

        /// <summary>
        /// 冲刺（包括地面冲刺和空中冲刺）
        /// </summary>
        public virtual void Dash()
        {
            // 是否可以在空中冲刺
            var canAirDash = stats.current.canAirDash && !isGrounded &&
                airDashCounter < stats.current.allowedAirDashes;
            // 是否可以在地面冲刺
            var canGroundDash = stats.current.canGroundDash && isGrounded &&
                                Time.time - lastDashTime > stats.current.groundDashCoolDown;
            // 如果按下冲刺键，且符合条件
            if (inputs.GetDashDown() && (canAirDash || canGroundDash))
            {
                if (!isGrounded) airDashCounter++;  // 空中冲刺计数+1；
                lastGroundTime = Time.time;         // 记录冲刺时间
                states.Change<DashPlayerState>();   // 切换到冲刺状态
            }
        }

        public virtual void ResetAirSpins() => airSpinCounter = 0;

        /// <summary>
        /// 执行旋转动作
        /// </summary>
        public virtual void Spin()
        {
            // 空中旋转条件：允许空中旋转 && 未超过上限
            var canAirSpin = (isGrounded || stats.current.canAirSpin) &&
                airSpinCounter < stats.current.allowedAirSpins;

            // 满足旋转条件 + 没有持物 + 按下旋转键
            if (stats.current.canSpin && canAirSpin && !holding & inputs.GetSpinDown())
            {
                if (!isGrounded)
                {
                    airSpinCounter++; // 空中旋转次数 + 1；
                }

                states.Change<SpinPlayerState>();   // 切换到旋转状态
                playerEvents.OnSpin?.Invoke();      // 触发旋转事件
            }
        }

        /// <summary>
        /// 踩踏攻击（空中下踩攻击）
        /// </summary>
        public virtual void StompAttack()
        {
            if (!isGrounded && !holding && stats.current.canStompAttack && inputs.GetStompDown())
            {
                states.Change<StompPlayerState>();
            }
        }

        /// <summary>
        /// 重置跳跃计数
        /// </summary>
        public virtual void ResetJumps() => jumpCounter = 0;

        /// <summary>
        /// 设置跳跃次数
        /// </summary>
        /// <param name="count"></param>
        public virtual void SetJumps(int count) => jumpCounter = count;

        /// <summary>
        /// 设置皮肤模型的父物体
        /// </summary>
        /// <param name="parent"></param>
        public virtual void SetSkinParent(Transform parent)
        {
            if (skin)
            {
                skin.parent = parent;
            }
        }

        /// <summary>
        /// 重置皮肤的父物体
        /// </summary>
        public virtual void ResetSkinParent()
        {
            if (skin)
            {
                skin.parent = transform;
                skin.localPosition = m_skinInitialPosition;
                skin.localRotation = m_skinInitialRotation;
            }
        }

        public virtual void Fall()
        {
            if (!isGrounded)
            {
                states.Change<FallPlayerState>();
            }
        }

        /// <summary>
        /// 如果玩家不在地面上，切换到下落状态
        /// </summary>
        /// <param name="other"></param>
        public virtual void PushRigidbody(Collider other)
        {
            // 排除台阶上方的情况，检测目标是否是刚体
            if(!IsPointUnderStep(other.bounds.max) && other.TryGetComponent(out Rigidbody rigidbody))
            {
                // 推动力量与玩家的水平速度相关
                var force = LateralVelocity * stats.current.pushForce;
                // 模拟物理推力除以质量，乘上 deltaTime)
                rigidbody.velocity += force / rigidbody.mass * Time.deltaTime;
            }
        }

        public virtual void Jump()
        {
            // 是否可以进行二段 / 多段跳
            var canMultiJump = (jumpCounter > 0) && (jumpCounter < stats.current.multiJumps);
            // 土狼跳判定
            var canCoyoteJump = (jumpCounter == 0) &&
                (Time.time < lastGroundTime + stats.current.coyoteJumpThreshold);

            // 地面 / 轨道 / 多段跳 /土狼跳条件满足时才允许跳跃
            if (isGrounded || canMultiJump || canCoyoteJump)
            {
                if (inputs.GetJumpDown())
                {
                    Jump(stats.current.maxJumpHeight);
                }
            }

            // 松开跳跃键
            if (inputs.GetJumpUp() && jumpCounter > 0 &&
                VerticalVelocity.y > stats.current.minJumpHeight)
            {
                VerticalVelocity = Vector3.up * stats.current.minJumpHeight;
            }
        }

        public virtual void Jump(float height)
        {
            jumpCounter++;
            VerticalVelocity = Vector3.up * height;
            states.Change<FallPlayerState>();
            playerEvents.Onjump?.Invoke();
        }

        /// <summary>
        /// 滑翔
        /// </summary>
        public virtual void Glide()
        {
            if (!isGrounded && inputs.GetGlide() && VerticalVelocity.y <= 0 && stats.current.canGlide)
            {
                states.Change<GlidingPlayerState>();
            }
        }

        /// <summary>
        /// 贴墙下滑
        /// </summary>
        /// <param name="other"></param>
        public virtual void WallDrag(Collider other)
        {
            if (stats.current.canWallDrag && Velocity.y <= 0 &&
                !holding && !other.TryGetComponent<Rigidbody>(out _))
            {
                // 检测前方是否有可滑的墙体
                if (CapsuleCast(transform.forward, 0.25f, out var hit, stats.current.wallDragLayers) &&
                    !DetectingLedge(0.25f, height, out _))
                {
                    // 如果是平台，成为其子物体
                    if (hit.collider.CompareTag(GameTags.Platform))
                    {
                        transform.parent = hit.transform;
                    }

                    lastWallNormal = hit.normal; // 记录墙面法线
                    states.Change<WallDragPlayerState>();
                }
            }
        }

        /// <summary>
        /// 爬杆
        /// </summary>
        /// <param name="other"></param>
        public virtual void GrabPole(Collider other)
        {
            if(stats.current.canPoleClimb && Velocity.y <= 0 &&
                !holding && other.TryGetComponent(out Pole pole))
            {
                this.pole = pole;
                states.Change<PoleClimbingPlayerState>();
            }
        }

        /// <summary>
        /// 抓住悬崖（Ledge Grab）
        /// </summary>
        public virtual void LedgeGrab()
        {
            // 必须允许挂边，角色正在下落，没有拿东西，并且存在悬挂状态类
            // 同时检测到悬崖
            if(stats.current.canLedgeHang && Velocity.y < 0 &&
               !holding && states.ContainsStateOfType(typeof(LedgeHangingPlayerState)) &&
               DetectingLedge(stats.current.ledgeMaxForwardDistance,stats.current.ledgeMaxDownwardDistance,out var hit))
            {
                // 排除球体和胶囊体碰撞体（避免挂到错误的物体）
                if(!(hit.collider is CapsuleCollider) && !(hit.collider is SphereCollider))
                {
                    // 计算角色挂到悬崖的位置
                    var ledgeDistance = radius + stats.current.ledgeMaxForwardDistance;
                    var lateralOffset = transform.forward * ledgeDistance;
                    var verticalOffset = Vector3.down * height * 0.5f - center;

                    Velocity = Vector3.zero;
                    // 如果悬挂的时平台，角色会成为平台的子物体
                    transform.parent = hit.collider.CompareTag(GameTags.Platform) ? hit.transform : null;
                    // 定位角色到挂边的位置
                    transform.position = hit.point - lateralOffset + verticalOffset;

                    // 切换到悬挂状态
                    states.Change<LedgeHangingPlayerState>();
                    playerEvents.OnLedgeGrabbed?.Invoke();
                }
            }
        }

        /// <summary>
        /// 检测角色前方是否有可挂的 ledge （悬挂/平台边缘）
        /// </summary>
        /// <param name="forwardDistance">向前检测的最大距离</param>
        /// <param name="downwardDistance">向下检测的最大距离</param>
        /// <param name="ledgeHit">如果检测成功，返回被击中的 ledge 的信息</param>
        /// <returns>是否检测到 ledge</returns>
        protected virtual bool DetectingLedge(float forwardDistance,float downwardDistance,out RaycastHit ledgeHit)
        {
            // Unity 内置的碰撞体偏移量 + 自定义的位移修正
            // 用于避免射线检测时因浮点误差导致的“卡进墙里”现象
            var contactOffset = Physics.defaultContactOffset + positionDelta;

            // 前方检测的最大长度（角色半径 + 额外向前检测的距离）
            var ledgeMaxDistance = radius + forwardDistance;

            // 从角色中心向上的频移，代表“检测 ledge 的高度起点”
            // = 半个角色高度 + 接触修正
            var ledgeHeightOffset = height * 0.5f + contactOffset;

            // 角色局部坐标系中的“上方向偏移”向量
            // 表示从当前位置向上移动到“检测 ledge 顶部”的点
            var upwardOffset = transform.up * ledgeHeightOffset;

            // 角色局部坐标系中“前方向偏移”向量
            // 表示从当前位置向前探测 ledge 的距离
            var forwardOffset = transform.forward * ledgeMaxDistance;

            //----------- 前方/上方是否有阻挡的判断------------------------
            // 1.从角色“上偏移点”往前打射线，检查前方是否有障碍
            // 2.从前方一点点往上打射线，检查上方是否有障碍
            // 如果有障碍，说明角色不能挂边
            if(Physics.Raycast(position + upwardOffset,transform.forward,ledgeMaxDistance,
                Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore) || 
                Physics.Raycast(position + forwardOffset * .01f,transform.up,ledgeHeightOffset,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                // 没有检测到合适的ledge，返回false；
                ledgeHit = new RaycastHit();
                return false;
            }
            // -------------------- 向下检测 ledge -------------------
            // 起点 = 角色位置 + 上偏移 + 前偏移
            //   即：角色头顶高度，往前探到 ledge 边缘外的位置
            var origin = position + upwardOffset + forwardOffset;

            // 向下检测的射线长度（下探深度 = downwardDistance + 接触修正）
            var distance = downwardDistance + contactOffset;

            // 从 orgin 向下发射射线，判断是否击中可以挂的 ledge
            // stats.current.ledgeHangingLayers 表示“允许挂边的层”
            return Physics.Raycast(origin, Vector3.down, out ledgeHit, distance, stats.current.ledgeHangingLayers, 
                                    QueryTriggerInteraction.Ignore);

        }

        public virtual void DirectionalJump(Vector3 direction, float height,float distance)
        {
            jumpCounter++;
            VerticalVelocity = Vector3.up * height; // 垂直上升
            LateralVelocity = direction * distance; // 水平方向推动
            playerEvents.Onjump?.Invoke();
        }

        #region 游泳相关
        /// <summary>
        /// 在指定方向上平滑移动玩家(水下参数)
        /// </summary>
        /// <param name="direction"></param>
        public virtual void WaterAcceleration(Vector3 direction) =>
            Accelerate(
                direction,
                stats.current.waterTurningDrag,
                stats.current.swimAcceleration,
                stats.current.swimTopSpeed);

        public virtual void WaterFaceDirection(Vector3 direction) =>
            FaceDirection(direction, stats.current.waterRotationSpeed);

        /// <summary>
        /// 进入水中（切换到游泳状态）
        /// </summary>
        /// <param name="other">水的碰撞体 </param>
        public virtual void EnterWater(Collider water)
        {
            if (!onWater && !health.isEmpty)
            {
                // Throw();        // 丢掉手中的物品
                onWater = true;
                this.water = water;
                states.Change<SwimPlayerState>(); // 切换到游泳状态
            }
        }

        /// <summary>
        /// 离开水域
        /// </summary>
        public virtual void ExitWater()
        {
            if (onWater)
            {
                onWater = false;
            }
        }

        /// <summary>
        /// 触发检测(玩家停留在触发器中)
        /// 用于检测是否进入水体或离开水体
        /// </summary>
        /// <param name="other"></param>
        public virtual void OnTriggerStay(Collider other)
        {
            // 如果当前·不在水中，但进入了水体包围盒
            if (other.CompareTag(GameTags.VolumeWater))
            {
                // 如果当前不在水中，但进入了水体包围盒
                if (!onWater && other.bounds.Contains(unsizedPosition))
                {
                    EnterWater(other);
                }
                else if (onWater) // 如果已经在水中，则检测是否离开
                {
                    // 计算一个向下偏移量，判断是否离开水面
                    var exitPoint = position + Vector3.down * k_waterExitOffset;
                    if (!other.bounds.Contains(exitPoint))
                    {
                        ExitWater();
                    }
                }
            }
        }
        #endregion
    }
}
