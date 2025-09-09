namespace GensouSakuya.QQBot.Agent
{
    public interface IQQBotAgent
    {
        Task<string> ChatOneTimeWithAgent(string agentId, string text);
    }
}
