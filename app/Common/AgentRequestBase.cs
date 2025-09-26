namespace app.Common
{
    public enum WalletType
    {
        Transfer = 0, // 轉帳錢包
        Single = 1    // 單一錢包
    }

    public class AgentRequestBase
    {
        public string agentId { get; set; } = string.Empty;
        public WalletType walletType { get; set; } = WalletType.Single;
    }
}
