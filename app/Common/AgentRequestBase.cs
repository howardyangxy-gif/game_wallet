namespace app.Common
{
    public enum WalletType
    {
        Transfer = 0, // 轉帳錢包
        Single = 1    // 單一錢包
    }

    public class AgentRequestBase
    {
        public int agentId { get; set; } = 0;
        public WalletType walletType { get; set; } = WalletType.Single;
    }
}
