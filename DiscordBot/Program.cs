﻿using System.IO;
using System;
using Discord;

class Program
{
    static void Main(string[] args) => new Program().Start();


    private DiscordClient _client;

    string[] greetings = { "hey", "hi", "hello" };



    //bot commands

    string[] Commands = { ".quote", ".seen", ".time" };
    enum ECommands { QUOTE, SEEN, TIME };


    //gets a line at a given index in a file
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


    //commands which don't require a nickname like .help, .time, .uptime, etc...
    bool IsCommand(string message, ECommands command)
    {
        if (message == Commands[(int)command])
            return true;

        return false;
    }

    //commands which contain a nickname like .quote newt, .seen newt, .burn newt, etc..
    bool IsCommand(string message, ECommands command, out string output_Nickname)
    {
        //splits the message into a array of words where delimiter is a space
        //and checks if the first word is a command
        //if true, 
        //returns all other words combined into a string
        //for example - .seen some thing
        //here there are 3 words separated by space - ".seen", "some" and "thing".
        //since the word[0] here is ".seen" i.e a command, this function will set output_Nickname to "some" and "thing" combined into a string with a space in between and will return true

        output_Nickname = "";

        char[] DelimeterChars = { ' ' };
        string[] words = message.Split(DelimeterChars);

        //if the first word is a command
        if (words[0] == Commands[(int)command])
        {
            //to support space in between nicknames like ".quote The Comet"
            if (words.Length > 2)
            {
                //here int i = 1 cause we don't want to include first word (i.e a command) into output_Nickname
                for (int i = 1; i < words.Length; ++i)
                {

                    //add space between each word
                    output_Nickname += words[i];

                    //don't add a space at the end
                    if (i != words.Length - 1)
                    output_Nickname += " ";
                }
            }
            //incase there's no space in the nickname like ".quote newt"
            else if (words.Length == 2)
            {
                output_Nickname = words[1];
            }

            return true;
        }

        return false;
    }


    public void Start()
    {
        Console.Title = "newt-bot";
        

        _client = new DiscordClient();

        _client.MessageReceived += async (s, e) =>
        {
            
            
            if (!e.Message.IsAuthor)
            {

                #region Log Everything On Console

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

                #region Record Messages


                bool RecordThisMessage = true;

                //quick hack to make sure that the message isn't a command
                foreach (string c in Commands)
                {
                    if (ICS_Contains(e.Message.Text, c))
                    {
                        RecordThisMessage = false;
                    }
                }


                string FileName = e.Message.User.Name + ".txt";


                if (RecordThisMessage)
                {

                    StreamWriter file = new StreamWriter(FileName, true);
                    file.WriteLine(e.Message.Text);
                    file.Close();

                    StreamWriter channel = new StreamWriter(FileName + "_channel.txt", true);
                    channel.WriteLine(e.Message.Channel.Name);
                    channel.Close();
                }
                
                
                #endregion

                #region Is Mentioning Me
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

                #region Swear at users

                if (ICS_Contains(e.Message.Text, "fuck") && (ICS_Contains(e.Message.Text, "newt-bot")|| ICS_Contains(e.Message.Text, "newt bot") || ICS_Contains(e.Message.Text, "newtbot")))
                {

                    await e.Channel.SendMessage("You need to shut up, " + e.Message.User.Name + ".");
                }

                #endregion

                #region Quote a random quote

                string out_NICK;
                if (IsCommand(e.Message.Text, ECommands.QUOTE, out out_NICK))
                {
                   if (File.Exists(out_NICK + ".txt"))
                    {
                        var NumberOfQuotes = File.ReadAllLines(out_NICK + ".txt").Length;

                        Random r = new Random();
                        int i = r.Next(0, NumberOfQuotes);

                        string Quote = GetLine(out_NICK + ".txt", i);
                        string channel = GetLine(out_NICK + ".txt" + "_channel.txt", i);

                        await e.Channel.SendMessage(@e.Message.User.Name + " once said in #" + channel + ": " + "\"" + Quote + "\"");

                    }
                   else
                    {
                        await e.Channel.SendMessage("I don't know any quotes from " + out_NICK + ".");
                    }
                }



                #endregion

                #region Quote Last Quote (When was this user last seen)

                if (IsCommand(e.Message.Text, ECommands.SEEN, out out_NICK))
                {
                    if (File.Exists(out_NICK + ".txt"))
                    {
                        var NumberOfQuotes = File.ReadAllLines(out_NICK + ".txt").Length;

                        string Quote = GetLine(out_NICK + ".txt", NumberOfQuotes - 1);
                        string channel = GetLine(out_NICK + ".txt" + "_channel.txt", NumberOfQuotes - 1);

                        await e.Channel.SendMessage(@e.Message.User.Name + " was last seen in #" + channel + " saying" + ": " + "\"" + Quote + "\"");

                    }
                    else
                    {
                        await e.Channel.SendMessage(out_NICK + " has never been seen.");
                    }
                }

                #endregion

                #region Get current time of the day

                if (IsCommand(e.Message.Text, ECommands.TIME))
                {
                    await e.Channel.SendMessage(DateTime.Now.ToString("h:mm:ss tt"));
                }

                #endregion


            }
        };

        _client.ExecuteAndWait(async () => 
        {
            await _client.Connect("MjczNDQ1Nzc3NjE0MDQ1MTg0.C2jzMg.hZsJqNIM8U20BDhNDYMlfX9PJj8", TokenType.Bot);
        });
    }


}