using ConvenienceMethods;
using NepBot.Resources.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace NepBot.Resources.Games
{
    public sealed class BlackjackGame
    {
        public readonly ulong _playerID;
        private GraphicsMethods _gm = new GraphicsMethods();
        private ImageData _imgData = new ImageData();
        private List<ACard> _cards;
        public List<ACard> _playerHand = new List<ACard>();
        public List<ACard> _dealerHand = new List<ACard>();
        ulong _totalBet;
        bool _playerEnd = false;
        public ulong TotalBet => _totalBet;

        public Discord.Rest.RestUserMessage HandMsg { get; set; }

        public bool DealerTurnEnd
        {
            get { return DealerTotal >= 17; }
        }

        public void EndPlayerTurn(bool yes)
        {
            _playerEnd = yes;
        }

        public bool GameEnd()
        {
            return WhoWon() > 0;
        }

        public bool DealerBlackjack
        {
            get { return _dealerHand.Count == 2 && DealerTotal == 21; }
        }

        // 0 no winner yet, 1 player, 2 dealer
        public int WhoWon()
        {
            if (_playerEnd == false)
                return 0;
            if (Busted(PlayerTotal, 22))
                return 2;
            else if (Busted(DealerTotal, 18))
                return 1;
            else if (PlayerTotal > DealerTotal)
                return 1;
            else if (DealerTotal > PlayerTotal)
                return 2;
            else if (DealerTotal == PlayerTotal)
                return 2;
            else if (_dealerHand.Count == 2 && _playerHand.Count == 2 && DealerTotal == 21)
                return 2;
            return 0;
        }

        public bool Busted(int totalValue, int bustAmt)
        {
            return totalValue >= bustAmt;
        }

        private int AceChanger(List<ACard> ac, bool isDealer)
        {
            int tot;
            while (true)
            {
                tot = 0;
                foreach (ACard g in ac)
                {
                    tot += g.CardValue;
                }
                if (Busted(tot, (isDealer) ? 18 : 22))
                {
                    tot = 0;
                    foreach (ACard f in ac)
                    {
                        f.aceToOne = true;
                        tot += f.CardValue;

                    }
                }
                break;
            }
            return tot;
        }

        public int PlayerTotal
        {
            get
            {
                return AceChanger(_playerHand, false);
            }
        }

        public int DealerTotal
        {
            get
            {
                return AceChanger(_dealerHand, true);
            }
        }

        public BlackjackGame(ulong playerID, ulong betAmt)
        {
            _playerID = playerID;
            _cards = _imgData.allCards;
            _cards.Shuffle();
            _cards.Shuffle();
            _cards.Shuffle();
            _cards.Shuffle();
            DealCards();
            _totalBet = betAmt;
        }

        private void DealCards()
        {
            _playerHand.Add(DealACard());
            _dealerHand.Add(DealACard());
            _playerHand.Add(DealACard());
            _dealerHand.Add(DealACard());
        }

        private ACard DealACard()
        {
            ACard g = _cards[0];
            _cards.RemoveAt(0);
            return g;
        }


        public void DrawCard(bool isPlayer)
        {
            if (isPlayer && !_playerEnd)
                _playerHand.Add(DealACard());
            else if (!isPlayer && !DealerTurnEnd)
                _dealerHand.Add(DealACard());
        }

        public void PlaceBet(ulong betAmt)
        {
            _totalBet += betAmt;
        }
    }
}
public static class ThreadSafeRandom
{
    [ThreadStatic] private static Random Local;

    public static Random ThisThreadsRandom
    {
        get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
    }
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}