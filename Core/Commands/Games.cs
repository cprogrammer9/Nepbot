using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Collections.Generic;
using ConvenienceMethods;
using Discord.WebSocket;
using NepBot.Data;
using NepBot.Resources.Games;
using NepBot.Resources.Code_Implements;
using NepBot.Resources.Extensions;
using System.DrawingCore;
using NepBot.Resources.Database;
using System.Text;
using System.IO;

namespace NepBot.Core.Commands
{
    public class Games : ModuleBase<SocketCommandContext>
    {
        MathCalculations _math = new MathCalculations();

        #region betting
        /*
        /// <summary>
        /// Work in progress.
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        [Command("Make Pool")]
        [Summary("(Not implemented yet) It's easy! Type !nep Make Pool PoolName|PoolBetAmt the maximum bet is 5000 to prevent abuse! Every user who bets in the pool will need to have 5000 pudding or whatever the amount is you chose")]

        public async Task MakePool([Remainder] string Input = null)
        {
            return;
            if (Input.ToLower() == "help")
            {
                await Context.Channel.SendMessageAsync("It's easy! Type !nep Make Pool PoolName|PoolBetAmt the maximum bet is 5000 to prevent abuse! Every user who bets in the pool will need to have 5000 pudding or whatever the amount is you chose");
                return;
            }
            var f = ExtensionMethods.GenericSplit(Input, "|", "&");
            if (int.Parse(f[1]) > 5000)
            {
                await Context.Channel.SendMessageAsync("5000 pudding is the maximum bet a pool can have.");
                return;
            }
            Program.AllGuildData[0].bets.Add(new GuildData.Bets(f[0], Context.User.Id, Context.Guild.Id, int.Parse(f[1])));
            await Context.Channel.SendMessageAsync($"All set! {Program.AllGuildData[0].bets[Program.AllGuildData[0].bets.Count - 1].BetInfo()}");
        }*/

        #endregion

        #region blackjack
        [Command("Stand")]
        [Summary("Keep your cards in your Blackjack game.")]
        public async Task Stand()
        {
            BlackjackGame agame = null;
            int listLocation = 0;
            foreach (BlackjackGame b in bjg)
            {
                if (Context.User.Id == b._playerID)
                {
                    agame = b;
                    break;
                }
                listLocation++;
            }
            if (agame == null)
            {
                await Context.Channel.SendMessageAsync("Silly, you ain't even playing Blackjack! Type !nep play blackjack to start! ~nepu");
                return;
            }
            agame.EndPlayerTurn(true);
            GraphicsMethods gm = new GraphicsMethods();
            while (true)
            {
                agame.DrawCard(false);
                if (agame.DealerTurnEnd)
                    break;

            }
            await agame.HandMsg.DeleteAsync();
            await Context.Channel.SendFileAsync(gm.CardLayout(agame._playerHand, agame._dealerHand, Context.User.Username), string.Concat("[Dealer:] ", agame.DealerTotal, " [Player:] ", agame.PlayerTotal, " [Bet Amt:] ", agame.TotalBet));
            if (agame.GameEnd())
            {
                SocketUser contUser = Context.User;
                UserData ud = ExtensionMethods.FindPerson(contUser.Id);
                bjg.RemoveAt(listLocation);
                if (agame.WhoWon() == 1)
                {
                    EndBlackjack(Context.User.Id, agame.TotalBet, true);
                    await Context.Channel.SendMessageAsync(string.Concat($"{ExtensionMethods.NeptuneEmojis(false)} You won, congrats and all that stuff, now how 'bout buying me a pudding? ~nepu By the way! You have ", ud.Pudding, " pudding now!"));
                }
                if (agame.WhoWon() == 2)
                {
                    EndBlackjack(Context.User.Id, agame.TotalBet, false);
                    await Context.Channel.SendMessageAsync(string.Concat($"{ExtensionMethods.NeptuneEmojis(true)} Yay I won!! Ahem suck to be you! Your {agame.TotalBet} belongs to me now! You have ", ud.Pudding, " pudding left!"));
                }
            }
        }

        [Command("hit me")]
        [Alias("hit")]
        [Summary("Draw a new card in your Blackjack game.")]
        public async Task DealCard()
        {
            BlackjackGame agame = null;
            int listLocation = 0;
            foreach (BlackjackGame b in bjg)
            {
                if (Context.User.Id == b._playerID)
                {
                    agame = b;
                    break;
                }
                listLocation++;
            }
            if (agame == null)
            {
                await Context.Channel.SendMessageAsync("Silly, you ain't even playing Blackjack! Type !nep play blackjack to start! ~nepu");
                return;
            }
            agame.DrawCard(true);
            GraphicsMethods gm = new GraphicsMethods();
            var kg = await Context.Channel.SendFileAsync(gm.CardLayout(agame._playerHand, agame._dealerHand, Context.User.Username), string.Concat("[Dealer:] ", agame.DealerTotal, " [Player:] ", agame.PlayerTotal, " [Bet Amt:] ", agame.TotalBet));
            //await Context.Channel.SendMessageAsync("Player Total: " + agame.PlayerTotal.ToString());
            await agame.HandMsg.DeleteAsync();
            agame.HandMsg = kg;
            if (agame.PlayerTotal > 21)
                agame.EndPlayerTurn(true);
            if (agame.GameEnd())
            {
                SocketUser contUser = Context.User;
                UserData ud = ExtensionMethods.FindPerson(contUser.Id);
                bjg.RemoveAt(listLocation);
                if (agame.WhoWon() == 1)
                {
                    EndBlackjack(Context.User.Id, agame.TotalBet, true);
                    await Context.Channel.SendMessageAsync(string.Concat($"{ExtensionMethods.NeptuneEmojis(false)} You won, congrats and all that stuff, now how 'bout buying me a pudding? ~nepu By the way! You have ", ud.Pudding, " pudding now!"));
                }
                if (agame.WhoWon() == 2)
                {
                    EndBlackjack(Context.User.Id, agame.TotalBet, false);
                    await Context.Channel.SendMessageAsync(string.Concat($"{ExtensionMethods.NeptuneEmojis(true)} Yay I won!! Ahem suck to be you! Your {agame.TotalBet} belongs to me now! You have ", ud.Pudding, " pudding left!"));
                }
                return;
            }


        }

        [Command("dice")]
        [Summary("Dice playing game. !nep dice (bet amt). Maximum bet is (25 + amount of cards owned) * your maximum OOC chat level!")]

        public async Task Dice([Remainder] string Input = null)
        {
            try
            {
                SocketUser contUser = Context.User;
                UserData ud = ExtensionMethods.FindPerson(contUser.Id);
                if (Input == null)
                {
                    await Context.Channel.SendMessageAsync(string.Concat("You forgot to place a bet! Bet some pudding will ya? Minimum bet is 10. Maximum bet is (25 + amount of cards owned (from the nepbot card collection game!)) * your non-RP level which for you is: ", _math.TotalBet(ud.NonLevel, ud).ToString()));
                    return;
                }
                ulong betAmt = 0;
                if (!ulong.TryParse(Input, out betAmt))
                {
                    await Context.Channel.SendMessageAsync("Umm... you have to enter ONLY a number as a bet amount! !nep dice (bet amount).");
                    return;
                }
                if (!_math.CanBet(betAmt, ud.Pudding) || betAmt > _math.TotalBet(ud.NonLevel, ud))
                {
                    await Context.Channel.SendMessageAsync(string.Concat("Aww you only have ", ud.Pudding.ToString(), ". Minimum bet is 10. Maximum bet is (25 + amount of cards owned (from the nepbot card collection game!)) * your non-RP level which is: ", _math.TotalBet(ud.NonLevel, ud).ToString()));
                    return;
                }
                string[] diceStrings = new string[]
                {
                    "",
                    "<:1_:574108098424471572>",
                    "<:2_:574108098462089216>",
                    "<:3_:574108098508357632>",
                    "<:4_:574108098441248768>",
                    "<:5_:574108098353037312>",
                    "<:6_:574108098386460672>"
                };
                int dealerTotal = 0;
                int playerTotal = 0;
                StringBuilder dealerString = new StringBuilder();
                StringBuilder playerString = new StringBuilder();
                for (int i = 0; i < 5; i++)
                {
                    int dRand = UtilityClass.ReturnRandom(1, diceStrings.Length);
                    int pRand = UtilityClass.ReturnRandom(1, diceStrings.Length);
                    dealerTotal += dRand;
                    dealerString.Append(diceStrings[dRand]).Append(" ");
                    playerTotal += pRand;
                    playerString.Append(diceStrings[pRand]).Append(" ");
                }
                string msg = string.Empty;
                if (dealerTotal > playerTotal)
                {
                    ud.Pudding -= betAmt;
                    msg = $"{ExtensionMethods.NeptuneEmojis(true)}**Neptune's Total: {dealerTotal}**\n{dealerString}\n**Your Total: {playerTotal}**\n{playerString}\nYou lost! Sorry pal, try again! You lost {betAmt} pudding! Your total pudding is now {ud.Pudding}.";
                }
                else if (playerTotal > dealerTotal)
                {
                    ud.Pudding += betAmt;
                    msg = $"{ExtensionMethods.NeptuneEmojis(false)}**Neptune's Total: {dealerTotal}**\n{dealerString}\n**Your Total: {playerTotal}**\n{playerString}\nYou win! Congrats! You won {betAmt} pudding! Your total pudding is now {ud.Pudding}.";
                }
                else if (playerTotal == dealerTotal)
                {
                    msg = $"{ExtensionMethods.NeptuneEmojis(false)}**Neptune's Total: {dealerTotal}**\n{dealerString}\n**Your Total: {playerTotal}**\n{playerString}\nIt was a tie... welp! Nothing lost, nothing gained! Your bet of {betAmt} pudding was returnd to you!";
                }
                await Context.Channel.SendMessageAsync(msg);
            }
            catch (Exception i)
            {
                Console.WriteLine(string.Format(">>> {0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}", new object[]
                {
                    i.Message,
                    i.TargetSite,
                    i.Source,
                    i.InnerException,
                    i.StackTrace,
                    i.HResult,
                    i.Data,
                    i.HelpLink
                }), false, null, null);
            }
        }

        public static List<BlackjackGame> bjg = new List<BlackjackGame>();

        [Command("play blackjack")]
        [Summary("Play blackjack. Type !nep play Blackjack (bet amount). Can only bet (25 + amount of cards owned) * your OOC level as maximum. Type !nep play blackjack help for more info.")]
        public async Task Blackjack([Remainder] string Input = null)
        {
            SocketUser contUser = Context.User;
            UserData ud = ExtensionMethods.FindPerson(contUser.Id);

            if (Input == null)
            {
                await Context.Channel.SendMessageAsync(string.Concat("You forgot to place a bet! Bet some pudding will ya? Minimum bet is 10. Maximum bet is (25 + amount of cards owned (from the nepbot card collection game!)) * your non-RP level which for you is: ", _math.TotalBet(ud.NonLevel, ud).ToString()));
                return;
            }

            if (Input.ToLower() == "help")
            {
                EmbedBuilder eb = new EmbedBuilder();
                eb.AddField("Play Blackjack", "enter the amount you wish to bet after blackjack. The maximum amount you can bet is (25 + amount of cards owned (from the nepbot card collection game!)) * your ooc chat level!");
                eb.AddField("Hit", "Draw a card.");
                eb.AddField("Stand", "Finish your turn with the cards you already have.");
                await Context.Channel.SendMessageAsync("", false, eb.Build());
                //await Context.Channel.SendMessageAsync(string.Concat("Type !nep play blackjack (bet amt). Type !nep hit to draw a card and !nep stand to not draw a card. You win double your bet amt! Max bet amount is 25 * your non-RP chat level! If you bet higher than your maximum it will automatically adjust to your max.", "Maximum bet is 25 * your non - RP level which for you is: ", _math.TotalBet(ud.NonLevel).ToString()));
                return;
            }
            BlackjackGame gameChecker = null;
            int listLocation = 0;
            foreach (BlackjackGame b in bjg)
            {
                if (Context.User.Id == b._playerID)
                {
                    gameChecker = b;
                    break;
                }
                listLocation++;
            }
            if (gameChecker != null)
            {
                await Context.Channel.SendMessageAsync("You're already playing blackjack! One game at a time folks! ~nepu");
                return;
            }
            try
            {
                if (ulong.Parse(Input) < 10)
                {
                    await Context.Channel.SendMessageAsync("10 puddings is the minimum bet! Please bet a minimum of 10 puddings before I eat them all!");
                    return;
                }
            }
            catch (Exception)
            {

            }
            ulong betAmt = ulong.Parse(Input);

            if (!_math.CanBet(betAmt, ud.Pudding) || betAmt > _math.TotalBet(ud.NonLevel, ud))
            {
                await Context.Channel.SendMessageAsync(string.Concat("Aww you only have ", ud.Pudding.ToString(), ". Minimum bet is 10. Maximum bet is (25 + amount of cards owned (from the nepbot card collection game!)) * your non-RP level which is: ", _math.TotalBet(ud.NonLevel, ud).ToString()));
                return;
            }

            GraphicsMethods gm = new GraphicsMethods();
            BlackjackGame agame = new BlackjackGame(Context.User.Id, betAmt);
            if (agame.DealerBlackjack)
            {
                EndBlackjack(Context.User.Id, agame.TotalBet, false);
                await Context.Channel.SendFileAsync(gm.CardLayout(agame._playerHand, agame._dealerHand, Context.User.Username), string.Concat("[Dealer:] ", agame.DealerTotal, " [Player:] ", agame.PlayerTotal, " [Bet Amt:] ", agame.TotalBet));
                await Context.Channel.SendMessageAsync(string.Concat($"{ExtensionMethods.NeptuneEmojis(true)} Yay I won!! Ahem suck to be you! Your {agame.TotalBet} belongs to me now! You have ", ud.Pudding, " pudding left!"));
                return;
            }
            bjg.Add(agame);
            Discord.Rest.RestUserMessage k = await Context.Channel.SendFileAsync(gm.CardLayout(agame._playerHand, agame._dealerHand, Context.User.Username), string.Concat("[Neptune's Hand:] ", agame.DealerTotal, " [Player:] ", agame.PlayerTotal, " [Bet Amt:] ", agame.TotalBet));
            agame.HandMsg = k;
        }

        private void EndBlackjack(ulong socketUserId, ulong betAmt, bool wonGame)
        {
            UserData ud = ExtensionMethods.FindPerson(socketUserId);
            if (wonGame)
                ud.Pudding += betAmt * 2;
            else
                ud.Pudding -= betAmt;
        }
        #endregion

        [Command("Owned Cards")]
        [Alias("oc")]
        [Summary("Displays all cards you own. Type the card name within 1 minute of using this command to view it. You can view other people's cards by entering their name after !nep owned cards")]
        public async Task OwnedCards([Remainder] string Input = null)
        {
            try
            {
                EmbedBuilder b = new EmbedBuilder();
                StringBuilder csb = new StringBuilder();
                StringBuilder rsb = new StringBuilder();
                StringBuilder srsb = new StringBuilder();
                StringBuilder ssrsb = new StringBuilder();
                SocketUser uf = (Input != null) ? ExtensionMethods.GetSocketUser(Input, Context, false) : null;
                UserData ud = ExtensionMethods.FindPerson((uf == null) ? Context.User.Id : uf.Id);

                List<CardTypes> ct = ud.PopulateList();
                foreach (CardTypes t in ct)
                {
                    if (t._cardType == CharacterCards.Type.c)
                        csb.Append($"{t.name}, ");
                    else if (t._cardType == CharacterCards.Type.r)
                        rsb.Append($"{t.name}, ");
                    else if (t._cardType == CharacterCards.Type.sr)
                        srsb.Append($"{t.name}, ");
                    else if (t._cardType == CharacterCards.Type.ssr)
                        ssrsb.Append($"{t.name}, ");
                }
                b.AddField($"Cards you owned sorted by rarity.\nYou own {ud.OwnedCards} / {CharacterCards.CardsCount}", "\nYou can view the card by typing the name in within 1 minute after using this bot command.");
                if (csb != null && csb.ToString() != string.Empty)
                    b.AddField("Common", $"{csb.ToString()}");

                if (rsb != null && rsb.ToString() != string.Empty)
                    b.AddField("Rare", $"{rsb.ToString()}");

                if (srsb != null && srsb.ToString() != string.Empty)
                    b.AddField("Super Rare", $"{srsb.ToString()}");

                if (ssrsb != null && ssrsb.ToString() != string.Empty)
                    b.AddField("Super Super Rare", $"{ssrsb.ToString()}");

                ud.StartSession();
                await Context.Channel.SendMessageAsync("", false, b.Build());
            }
            catch (Exception n)
            {
                ExtensionMethods.WriteToLog(ExtensionMethods.LogType.ErrorLog, n, "");
            }
        }

        private ulong TotalLoops(ulong totalAmt, ulong pudding, ulong cost)
        {
            if (cost > pudding)
                return 0;
            if (totalAmt == 0)
                return 1;
            if (cost * totalAmt > pudding)
                return (pudding > cost) ? pudding / cost : cost / pudding;
            return totalAmt;
        }

        [Command("buy card")]
        [Alias("bc")]
        [Summary("Buys a card. Type !nep buy card or (bc) and the number of cards you want to buy. It will automatically buy as many as you can buy if you attempt to buy more than you can afford. 1000 pudding per card.")]
        public async Task Gacha([Remainder] string Input = null)
        {
            try
            {
                UserData ud = ExtensionMethods.FindPerson(Context.User.Id);
                if (ud.OwnedCards == CharacterCards.CardsCount)
                {
                    await Context.Channel.SendMessageAsync("You own them all champ! Nag admin to make more. Admin doesn't do anything unless nagged.");
                    return;
                }
                ulong cost = 3000;
                if (ud.Pudding < cost)
                {
                    await Context.Channel.SendMessageAsync($"You don't have {cost} pudding! {cost} is required to play the Gacha!");
                    return;
                }
                int totalAmt = 0;
                int.TryParse(Input, out totalAmt);
                if (totalAmt < 0)
                    totalAmt = 0;
                StringBuilder sb = new StringBuilder();
                CardTypes ct = Program.characterCards.RandomCard();
                double d = ud.Pudding;
                ulong totalLoops = TotalLoops((ulong)totalAmt, (ulong)d, cost);//(ud.Pudding < 2500) ? (int)(d / cost) : totalAmt;
                totalAmt = (int)totalLoops;
                List<string> cardTypes = new List<string>();
                List<int> amounts = new List<int>();
                List<string> newcards = new List<string>();
                await Context.Channel.SendMessageAsync(totalLoops.ToString());
                if (totalAmt > 5)
                {
                    ud.Pudding -= cost * (ulong)totalAmt;
                    while (totalAmt > 0)
                    {
                        if (!cardTypes.Contains(ct.name))
                        {
                            cardTypes.Add(ct.name);
                            amounts.Add(1);
                        }
                        else
                        {
                            for (int i = 0; i < cardTypes.Count; i++)
                            {
                                if (cardTypes[i] == ct.name)
                                {
                                    amounts[i]++;
                                    break;
                                }
                            }
                        }
                        if (ud.AddToList(ct.idNumber))
                        {
                            newcards.Add(ct.name);
                        }
                        totalAmt--;
                        ct = Program.characterCards.RandomCard();
                    }
                    StringBuilder sb2 = new StringBuilder();
                    for (int i = 0; i < cardTypes.Count; i++)
                    {
                        sb2.Append($"You got: {amounts[i]} of {cardTypes[i]}\n");
                    }
                    sb2.Append("New Cards:\n");
                    foreach (string g in newcards)
                        sb2.Append($"{g}\n");
                    await Context.Channel.SendMessageAsync(sb2.ToString());
                    return;
                }
                if (totalAmt > 0)
                {
                    d -= totalAmt * (double)cost;
                }
                else
                    d -= cost;
                if (d < 0)
                {
                    await Context.Channel.SendMessageAsync("D is less than 0. Something is wrong with your math Admin, git gud.");
                    d = 0;
                }
                ud.Pudding = (ulong)d;
                if (!ud.AddToList(ct.idNumber))
                    sb.Append($"You already have this card ({ct.name})! You can't have 2 so I'll just... *poof* it's gone now. You have {ud.Pudding.ToString()} pudding left! ").Append(ImgurImplement.GetImage(ct.imgurID).Result).Append("\n");
                else
                    sb.Append($"You got a new card ({ct.name})! YAY! Congrats 'n stuff. You have {ud.Pudding.ToString()} pudding left! ").Append(ImgurImplement.GetImage(ct.imgurID).Result).Append("\n");
                totalAmt--;
                while (totalAmt > 0)
                {
                    ct = Program.characterCards.RandomCard();
                    if (!ud.AddToList(ct.idNumber))
                        sb.Append($"You already have this card ({ct.name})! You can't have 2 so I'll just... *poof* it's gone now. You have {ud.Pudding.ToString()} pudding left! ").Append(ImgurImplement.GetImage(ct.imgurID).Result).Append("\n");
                    else
                        sb.Append($"You got a new card ({ct.name})! YAY! Congrats 'n stuff. You have {ud.Pudding.ToString()} pudding left! ").Append(ImgurImplement.GetImage(ct.imgurID).Result).Append("\n");
                    totalAmt--;
                }
                await Context.Channel.SendMessageAsync(sb.ToString());
            }
            catch (Exception m)
            {
                await Context.Channel.SendMessageAsync($"{m.Message}\n{ m.TargetSite}\n{ m.Source}\n{ m.InnerException}\n{ m.StackTrace}\n{ m.HResult}\n{ m.Data}\n{ m.HelpLink}");
            }
        }

    }
}
