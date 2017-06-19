using System;

namespace Common.Messages
{


    [Serializable]
    public class NextGatewayResponseServerMessage 
    {
        public string GatewayUrl { get; set; }
    }
    [Serializable]
    public class CreateNewGameRequest 
    {
        public string UserKey { get; set; }
        public string GameName { get; set; }
    }
    [Serializable]
    public class GameUpdateServerMessage 
    {
        public string GameId { get; set; }
        public GameStatus GameStatus { get; set; }
        public CardGameQuestionTransport Question { get; set; }
        public string UserKey { get; set; }
    }
    [Serializable]
    public class GameServerServerMessage 
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