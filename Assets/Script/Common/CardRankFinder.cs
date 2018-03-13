using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Msg;

namespace Pusoy
{
    using CardRankDictionary = Dictionary<CardRank, List<uint[]>>;

    class Card : IComparable
    {
        public Card(uint card)
        {
            Value = card % 13;
            Color = (CardColorSuit)(card / 13);
        }
        public uint Value { get; set; }
        public CardColorSuit Color { get; set; }
        public uint CardValue()
        {
            return (uint)Color * 13 + Value;
        }

        public int CompareTo(object obj)
        {
            Card card = (Card)obj;

            return Value == card.Value ? (Color > card.Color ? -1 : 1) : (Value > card.Value ? -1 : 1);
        }
    }
    class CardRankFinder
    {
        public const uint Card_A = 12;
        public const uint Card_10 = 8;
        public const uint Card_5 = 3;
        public const uint Card_2 = 0;

        private static readonly uint[] cardsNum = { 1, 2, 4, 3, 5, 5, 5, 4, 5 };
        public static CardRankDictionary Find(uint[] cards)
        {
            Card[] sortCards = new Card[cards.Length];
            for (int i = 0; i < cards.Length; i++)
            {
                sortCards[i] = new Card(cards[i]);
            }

            Array.Sort(sortCards);

            CardRankDictionary ret = new CardRankDictionary();
            for (CardRank rank = CardRank.StraightFlush; rank > CardRank.HighCard; rank--)
            {
                find(rank, ref sortCards, ref ret);
            }

            return ret;
        }

        private static void find(CardRank rank, ref Card[] cards, ref CardRankDictionary result)
        {
            if (cards.Length < cardsNum[Convert.ToInt32(rank)]) return;

            Card[] foundCards = new Card[cardsNum[Convert.ToInt32(rank)]];
            int found = 0;
            findRecursive(rank, ref cards, 0, ref foundCards, found, ref result);
        }

        private static void findRecursive(CardRank rank, ref Card[] cards, int pos, ref Card[] foundCards, int found, ref CardRankDictionary result)
        {
            for (int i = pos; i < cards.Length; i++)
            {
                Card card = cards[i];
                foundCards[found] = card;

                if (!match(rank, ref foundCards, found + 1))
                {
                    continue;
                }

                if (found + 1 == cardsNum[Convert.ToInt32(rank)])
                {
                    // save to result
                    List<uint[]> l;
                    uint[] saveCards = new uint[foundCards.Length];
                    for (int j = 0; j < foundCards.Length; j++)
                    {
                        saveCards[j] = foundCards[j].CardValue();
                    }
                    if (result.TryGetValue(rank, out l))
                    {
                        l.Add(saveCards);
                    }
                    else
                    {
                        l = new List<uint[]>();
                        l.Add(saveCards);
                        result[rank] = l;
                    }
                    continue;
                }

                findRecursive(rank, ref cards, i + 1, ref foundCards, found + 1, ref result);
            }
        }

        private static bool match(CardRank rank, ref Card[] cards, int found)
        {
            switch (rank)
            {
                case CardRank.StraightFlush:
                    {
                        for (int i = 0; i < found - 1; i++)
                        {
                            if (cards[i].Value == cards[i + 1].Value || cards[i].Color != cards[i + 1].Color)
                                return false;
                        }
                        if (found > 1 && cards[0].Value == Card_A)
                        {
                            if (cards[found - 1].Value != 5 - found && cards[found - 1].Value != 13 - found)
                                return false;

                            return true;
                        }
                        if (found > 1 && cards[0].Value - cards[found - 1].Value > found - 1)
                            return false;
                        return true;
                    }

                case CardRank.FourOfAKind:
                    {
                        int same = 0;
                        int diff = 0;
                        for (int i = 0; i < found - 1; i++)
                        {
                            if (cards[i].Value != cards[i + 1].Value)
                            {
                                diff++;
                                same = 0;
                                if (diff > 1)
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                same++;
                                if (same == 3)
                                {
                                    return true;
                                }
                            }
                        }
                        if (found == 4) return false;
                        return true;
                    }

                case CardRank.FullHouse:
                    {
                        int same = 0;
                        int diff = 0;
                        for (int i = 0; i < found - 1; i++)
                        {
                            if (cards[i].Value != cards[i + 1].Value)
                            {
                                diff++;
                                same = 0;
                                if (diff > 1) return false;
                            }
                            else
                            {
                                same++;
                                if (same > 2) return false;
                            }
                        }

                        return true;
                    }

                case CardRank.Flush:
                    {
                        for (int i = 0; i < found - 1; i++)
                        {
                            if (cards[i].Color != cards[i + 1].Color)
                                return false;
                        }
                        if (found == cardsNum[(int)CardRank.Flush] && match(CardRank.StraightFlush, ref cards, found)) return false;
                        

                        return true;
                    }

                case CardRank.Straight:
                    {
                        if (found == cardsNum[(int)CardRank.Flush] && match(CardRank.StraightFlush, ref cards, found)) return false;

                        for (int i = 0; i < found - 1; i++)
                        {
                            if (cards[i].Value == cards[i + 1].Value)
                                return false;
                        }
                        if (found > 1 && cards[0].Value == Card_A)
                        {
                            if (cards[found - 1].Value != 5 - found && cards[found - 1].Value != 13 - found)
                                return false;

                            return true;
                        }
                        if (found > 1 && cards[0].Value - cards[found - 1].Value > found - 1)
                            return false;

                        
                        return true;
                    }

                case CardRank.ThreeOfAKind:
                    {
                        int same = 0;
                        int diff = 0;
                        for(int i = 0; i < found - 1; i++)
                        {
                            if (cards[i].Value != cards[i + 1].Value)
                            {
                                diff++;
                                same = 0;
                                if (diff > 2) return false;
                            }
                            else
                            {
                                same++;
                                if (same == 2) return true;
                            }
                        }
                        if (found == cardsNum[(int)rank]) return false;
                        return true;
                    }

                case CardRank.TwoPair:
                    {
                        int same = 0;
                        int sametime = 0;
                        int diff = 0;
                        for (int i = 0; i < found - 1; i++)
                        {
                            if (cards[i].Value != cards[i + 1].Value)
                            {
                                diff++;
                                same = 0;
                                if (diff > 2) return false;
                            }
                            else
                            {
                                same++;
                                if (same == 1)
                                {
                                    sametime++;
                                    if (sametime == 2 && !match(CardRank.FourOfAKind,ref cards,found)) return true;
                                }
                            }
                        }
                        if (found == cardsNum[(int)rank]) return false;
                        return true;
                    }

                case CardRank.OnePair:
                    {
                        if (found == 2) return cards[0].Value == cards[1].Value;
                        return true;
                    }
            }
            return true;
        }
    }
}
