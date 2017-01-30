using System.IO;
using System;
using Discord;

class Program
{
    static void Main(string[] args) => new Program().Start();


    private DiscordClient _client;

    string[] greetings = { "hey", "hi", "hello", "sup" , "morgen"};


    string QuoteCommand = ".quote";


    string GetLine(string fileName, int line)
    {
        using (var sr = new StreamReader(fileName))
        {
            for (int i = 0; i < line; i++)
                sr.ReadLine();

            return sr.ReadLine();
        }
    }


    //ICS = In Case-Sensitive 
    bool ICS_Contains(string source, string toCheck)
    {
        return source != null && toCheck != null && source.IndexOf(toCheck, StringComparison.CurrentCultureIgnoreCase) >= 0;
    }


    public void Start()
    {
        Console.Title = "newt-bot";
        

        _client = new DiscordClient();

        _client.MessageReceived += async (s, e) =>
        {
            
            
            if (!e.Message.IsAuthor)
            {


                #region LogEverythingOnConsole

                //write user's name in a different color
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(e.User.Name);
                Console.ResetColor();

                //write channel's name in a different color
                Console.Write(" in ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("#" + e.Message.Channel.Name + ": ");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(e.Message.Text + "\n");
                Console.ResetColor();
                #endregion

                #region RecordMessages

                string FileName = e.Message.User.Name + ".txt";

                if (!(e.Message.Text.Contains(QuoteCommand)))
                {

                    StreamWriter file = new StreamWriter(FileName, true);
                    file.WriteLine(e.Message.Text);
                    file.Close();

                    StreamWriter channel = new StreamWriter(FileName + "_channel.txt", true);
                    channel.WriteLine(e.Message.Channel.Name);
                    channel.Close();
                }
                
                
                #endregion

                #region IsMentioningMe
                if (e.Message.IsMentioningMe())
                {
                    await e.Channel.SendMessage(e.Message.User.Name + ", why are you mentioning me?");
                }
                #endregion

                #region Greet Users
                foreach (string text in greetings)
                {
                    if (string.Equals(e.Message.Text, text, System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        await e.Channel.SendMessage("Hi " + e.Message.User.Name);
                    }
                }
                #endregion



                #region SwearAtUsers_Iff_TheySwearAtMe

                if (ICS_Contains(e.Message.Text, "fuck") && (ICS_Contains(e.Message.Text, "newt-bot")|| ICS_Contains(e.Message.Text, "newt bot") || ICS_Contains(e.Message.Text, "newtbot")))
                {
                    await e.Channel.SendMessage("You need to shut up, " + e.Message.User.Name + ".");
                }

                #endregion

                #region Quote a random quote


                //quoting yourself
                //===========================================================================================================//
                if (string.Equals(e.Message.Text, QuoteCommand, System.StringComparison.CurrentCultureIgnoreCase))
                {
                   if (File.Exists(FileName))
                    {
                        var NumberOfQuotes = File.ReadAllLines(FileName).Length;

                        Random r = new Random();
                        int i = r.Next(0, NumberOfQuotes);

                        string Quote = GetLine(FileName, i);
                        string channel = GetLine(FileName + "_channel.txt", i);

                        await e.Channel.SendMessage(@e.Message.User.Name + " once said in #" + channel + ": " + "\"" + Quote + "\"");

                    }
                   else
                    {
                        await e.Channel.SendMessage("I don't know any quotes from you.");
                    }
                }
                //===========================================================================================================//


                //quoting someone else
                //===========================================================================================================//
                string QuoteCommandOneSpace = QuoteCommand + " ";

                foreach (Discord.User user in e.Channel.Users)
                {
                    if (user.IsBot)
                        continue;

                    string t = QuoteCommandOneSpace + user.Name;

                    if (string.Equals(e.Message.Text, t, System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (File.Exists(user.Name + ".txt"))
                        {
                            var NumberOfQuotes = File.ReadAllLines(user.Name + ".txt").Length;

                            Random r = new Random();
                            int i = r.Next(0, NumberOfQuotes);

                            string Quote = GetLine(user.Name + ".txt", i);
                            string channel = GetLine(user.Name + ".txt" + "_channel.txt", i);

                            await e.Channel.SendMessage(@user.Name + " once said in #" + channel + ": " + "\"" + Quote + "\"");

                        }
                        else
                        {
                            await e.Channel.SendMessage("I don't know any quotes from " + user.Name + ".");
                        }
                    }

                }


                //===========================================================================================================//


                #endregion




            }
        };

        _client.ExecuteAndWait(async () => 
        {
            await _client.Connect("MjczNDQ1Nzc3NjE0MDQ1MTg0.C2jzMg.hZsJqNIM8U20BDhNDYMlfX9PJj8", TokenType.Bot);
        });
    }


}