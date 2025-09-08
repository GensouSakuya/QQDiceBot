namespace GensouSakuya.QQBot.Agent
{
    public interface IQQBotAgent
    {
        Task<string> ChatWithAgent(string agentId, string text);
    }
}
