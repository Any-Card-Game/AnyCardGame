using System;

namespace Common.Redis.RedisMessages
{
    [Serializable]
    public abstract class RedisMessage
    {
    }
    [Serializable]
    public class DefaultRedisMessage : RedisMessage
    {
    }

    [Serializable]
    public class NextGatewayResponseRedisMessage : RedisMessage
    {
        public string GatewayUrl { get; set; }
    }
    [Serializable]
    public class CreateNewGameRequest : RedisMessage
    {
        public string GatewayKey { get; set; }
        public string UserKey { get; set; }
        public string GameName { get; set; }
    }
    [Serializable]
    public class GameUpdateRedisMessage : RedisMessage
    {
        public string GameId { get; set; }
        public GameStatus GameStatus { get; set; }
        public CardGameQuestionTransport Question { get; set; }
        public string GameServer { get; set; }
        public string UserKey { get; set; }
    }
    [Serializable]
    public class GameServerRedisMessage : RedisMessage
    {
        public string GameId { get; set; }
        public int AnswerIndex { get; set; }
    }

    [Serializable]
    public class CardGameQuestionTransport
    {

        public string User { get; set; }
        public string Question { get; set; }
        public string[] Answers { get; set; }
    }
    [Serializable]
    public enum GameStatus
    {
        Started,
        AskQuestion,
        GameOver
    }


}