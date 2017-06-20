using DiscordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordBotCore.ChatBot
{
    class ChatBot
    {
        private List<Conversation> conversations;
        public ChatBot()
        {
            conversations = new List<Conversation>();
        }

        public string GetResponse(string input, string fromUserId)
        {
            input = PreprocessInput(input);
            UserInput LatestInput;
            using (var db = new BotContext())
            {
                LatestInput = db.UserInput.SingleOrDefault(x => x.Input == input);
                if (LatestInput == null)
                {
                    //the bot has not heard this one before
                    LatestInput = new UserInput
                    {
                        Id = Guid.NewGuid(),
                        Input = input
                    };
                    db.UserInput.Add(LatestInput);
                    db.SaveChanges();
                }
            }

            string response = "";

            UserInput PrevResponse = null;

            using (var db = new BotContext())
            {
                Conversation memory = conversations.FirstOrDefault(x => x.UserId == fromUserId);
                if (memory != null)
                {
                    string botLastReply = memory.LastBotResponse;
                    conversations.Remove(memory);
                    PrevResponse = db.UserInput.SingleOrDefault(x => x.Input == botLastReply);

                    if (PrevResponse != null)
                    {
                        UserInputRelationship relationship = db.UserInputRelationship.SingleOrDefault(x => x.BotOutputId == PrevResponse.Id && x.UserReplyId == LatestInput.Id);

                        if (relationship != null)
                        {
                            //this is an existing relationship
                            relationship.TimesReplied++;
                            db.UserInputRelationship.Update(relationship);
                        }
                        else
                        {
                            //this is a new relationship
                            relationship = new UserInputRelationship
                            {
                                Id = Guid.NewGuid(),
                                BotOutputId = PrevResponse.Id,
                                UserReplyId = LatestInput.Id,
                                TimesReplied = 1
                            };
                            db.UserInputRelationship.Add(relationship);
                        }
                        db.SaveChanges();
                    }
                }
                List<UserInputRelationship> matches = db.UserInputRelationship.Where(x => x.BotOutputId == LatestInput.Id).ToList();
                int matchesCount = matches.Count;
                if (matchesCount > 0)
                {
                    UserInputRelationship bestMatch = null;
                    if (matchesCount > 1)
                    {
                        //sort by our most compatible responses
                        matches.Sort((x, y) => x.TimesReplied.CompareTo(y.TimesReplied));

                        int topBest;

                        if(matchesCount >= 10)
                        {
                            topBest = 10;
                        }
                        else
                        {
                            topBest = matchesCount;
                        }

                        //get a random response from a subset of our better matches
                        Random rand = new Random();
                        int randIndex = rand.Next(0, topBest-1);
                        bestMatch = matches[randIndex];
                    }
                    else
                    {
                        bestMatch = matches[0];
                    }
                    if (bestMatch != null)
                    {
                        response = db.UserInput.SingleOrDefault(x => x.Id == bestMatch.UserReplyId).Input;
                    }
                }
            }





            //UserInput PrevResponse = null;
            //if (response != "")
            //{
            //    using (var db = new BotContext())
            //    {
            //        PrevResponse = db.UserInput.SingleOrDefault(x => x.Input == response);
            //    }
            //if (PrevResponse != null)
            //{
            //    using (var db = new BotContext())
            //    {
            //        UserInputRelationship relationship = db.UserInputRelationship.SingleOrDefault(x => x.BotOutputId == PrevResponse.Id && x.UserReplyId == LatestInput.Id);

            //        if (relationship != null)
            //        {
            //            //this is an existing relationship
            //            relationship.TimesReplied++;
            //            db.UserInputRelationship.Update(relationship);
            //        }
            //        else
            //        {
            //            //this is a new relationship
            //            relationship = new UserInputRelationship
            //            {
            //                Id = Guid.NewGuid(),
            //                BotOutputId = PrevResponse.Id,
            //                UserReplyId = LatestInput.Id,
            //                TimesReplied = 1
            //            };
            //            db.UserInputRelationship.Add(relationship);
            //        }
            //        db.SaveChanges();
            //    }
            //}
            //}

            //if (PrevResponse != null)
            //{
            //    List<UserInputRelationship> matches;
            //    using (var db = new BotContext())
            //    {
            //        matches = db.UserInputRelationship.Where(x => x.BotOutputId == PrevResponse.Id).ToList();
            //    }
            //    int matchesCount = matches.Count;
            //    if (matchesCount > 0)
            //    {
            //        UserInputRelationship bestMatch = null;
            //        if (matchesCount > 1)
            //        {
            //            matches.Sort((x, y) => x.TimesReplied.CompareTo(y.TimesReplied));

            //            Random rand = new Random();
            //            int randIndex = rand.Next(0, (matchesCount * (1 / 2)));
            //            bestMatch = matches[randIndex];
            //        }
            //        else
            //        {
            //            bestMatch = matches[0];
            //        }
            //        if (bestMatch != null)
            //        {
            //            using (var db = new BotContext())
            //            {
            //                response = db.UserInput.SingleOrDefault(x => x.Id == bestMatch.UserReplyId).Input;
            //            }
            //        }
            //    }
            //}

            if (response == "")
                response = RandomInput();
            if (response == "")
                response = "Hello";
            //}
            //catch (Exception ex)
            //{
            //    LogMessage logMessage = new LogMessage(LogSeverity.Critical, "GetResponse", ex.Message, ex);
            //    Log(logMessage);
            //    response = "I apologize, my speech processors appear to be malfunctioning.";
            //}

            conversations.Add(new Conversation(response, fromUserId));

            return response;
        }
        public string RandomInput()
        {
            string random = "";
            using (var db = new BotContext())
            {


                //complete random
                random = db.UserInput.OrderBy(o => Guid.NewGuid()).First().Input;
            }
            return random;
        }

        public string PreprocessInput(string input)
        {
            string processed = input;
            if (processed.First() == ' ')
            {
                processed = processed.Substring(1);
            }
            processed = processed.ToLower();

            return processed;
        }
    }
}
