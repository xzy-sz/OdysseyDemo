namespace Assets.PLAYER_TWO.Platformer_Project.Scripts.WayPoints
{
    public enum WaypointMode
    {
        Loop,           // 循环模式：达到最后一个路点然后从头开开始循环
        PingPong,       // 往返模式：到达末尾时反向移动，向乒乓球一样来回
        Once            // 单次模式：到达最后一个路点后停止
    }
}
