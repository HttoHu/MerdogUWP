using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorTools
{
    public static class Lexer
    {
        public enum Tag
        {
            ID, NUM, REAL, KEY
        }
        public static bool StringMode = false;
        public enum TokenColor
        {
            Default, Blue, Green, Pink, DarkRed, Yellow, Grey, Wheat
        }
        public static Dictionary<string, TokenColor> token_tag_dic = new Dictionary<string, TokenColor>();
        public struct pair
        {
            public Tag tag;
            public TokenColor col;
        }
        static public bool is_split_char(char ch)
        {
            switch (ch)
            {
                case ',':
                case ' ':
                case '.':
                case ';':
                case '[':
                case ']':
                case '+':
                case '-':
                case '*':
                case '/':
                case '=':
                case '<':
                case '\n':
                case '>':
                case '(':
                case ')':
                case '\r':
                case '\t':
                    return true;
                default:
                    return false;
            }
        }
        public static void init_lexer()
        {
            token_tag_dic.Add("if", TokenColor.Blue);
            token_tag_dic.Add("while", TokenColor.Blue);
            token_tag_dic.Add("else", TokenColor.Blue);
            token_tag_dic.Add("elif", TokenColor.Blue);
            token_tag_dic.Add("switch", TokenColor.Blue);
            token_tag_dic.Add("case", TokenColor.Blue);
            token_tag_dic.Add("do", TokenColor.Blue);
            token_tag_dic.Add("for", TokenColor.Blue);
            token_tag_dic.Add("break", TokenColor.Blue);
            token_tag_dic.Add("continue", TokenColor.Blue);
            token_tag_dic.Add("struct", TokenColor.Green);
            token_tag_dic.Add("enum", TokenColor.Green);
            token_tag_dic.Add("function", TokenColor.Green);
            token_tag_dic.Add("program", TokenColor.Green);
            token_tag_dic.Add("int", TokenColor.Pink);
            token_tag_dic.Add("return", TokenColor.Blue);
            token_tag_dic.Add("real", TokenColor.Pink);
            token_tag_dic.Add("char", TokenColor.Pink);
            token_tag_dic.Add("string", TokenColor.Pink);
            token_tag_dic.Add("bool", TokenColor.Pink);
            token_tag_dic.Add("void", TokenColor.Pink);
            token_tag_dic.Add("true", TokenColor.Blue);
            token_tag_dic.Add("false", TokenColor.Blue);
            token_tag_dic.Add("new", TokenColor.Blue);
            token_tag_dic["make"] = TokenColor.Blue;
            token_tag_dic["cast"] = TokenColor.Blue;
            token_tag_dic["std"] = TokenColor.Wheat;
            token_tag_dic["vector"] = TokenColor.Pink;
            token_tag_dic["map"] = TokenColor.Pink;
            token_tag_dic["import"] = TokenColor.Blue;
        }
        static private bool parse_word(string str)
        {
            if (str.Length == 0)
                return false;
            if (!Char.IsLetter(str[0]) && str[0] != '_')
                return false;
            for (int i = 1; i < str.Length; i++)
            {
                if (!Char.IsLetterOrDigit(str[i]))
                    return false;
            }
            return true;
        }
        public static TokenColor get_token_info(string str)
        {
            if (str == "")
                return TokenColor.Default;
            var result = token_tag_dic.ContainsKey(str);
            if (result == true)
            {
                return token_tag_dic[str];
            }
            if (parse_word(str))
            {
                return TokenColor.Grey;
            }
            switch (str[0])
            {
                case '\'':
                case '\"':
                    return TokenColor.DarkRed;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return TokenColor.DarkRed;
                default:
                    break;
            }
            return TokenColor.Default;
        }
    }
    public class Token
    {
        public Token(int s, int e, Lexer.TokenColor tc) { startPos = s; endPos = e; token_color = tc; }
        public Lexer.TokenColor token_color;
        public int startPos;
        public int endPos;
    }
    public class MLexer
    {
        public MLexer(string str) { content = str; }
        public List<Token> GetTokenStream()
        {
            List<Token> ret = new List<Token>();
            int last_pos = 0;
            int cur_pos = 0;
            while (cur_pos < content.Length)
            {
                //空白
                if (content[cur_pos] == ' ' || content[cur_pos] == '\n' || content[cur_pos] == '\r')
                {
                    if (last_pos == cur_pos)
                    {
                        cur_pos++;
                        last_pos = cur_pos;
                        continue;
                    }
                    else
                    {
                        ret.Add(new Token(last_pos, cur_pos,
                            Lexer.get_token_info(content.Substring(last_pos, cur_pos - last_pos
                            ))));
                        cur_pos++;
                        last_pos = cur_pos;
                        continue;
                    }
                }
                //注释
                else if (content[cur_pos] == '/')
                {
                    if (cur_pos + 1 < content.Length && content[cur_pos + 1] == '/')
                    {
                        ret.Add(new Token(last_pos, cur_pos,
                         Lexer.get_token_info(content.Substring(last_pos, cur_pos - last_pos))));
                        last_pos = cur_pos;
                        cur_pos += 2;
                        while (cur_pos < content.Length && content[cur_pos] != '\n' && content[cur_pos] != '\r')
                        {
                            cur_pos++;
                        }
                        ret.Add(new Token(last_pos, cur_pos, Lexer.TokenColor.Green));
                        last_pos = cur_pos;
                        continue;
                    }
                    else if (cur_pos + 1 < content.Length && content[cur_pos + 1] == '*')
                    {
                        ret.Add(new Token(last_pos, cur_pos, Lexer.get_token_info(content.Substring(last_pos, cur_pos - last_pos))));
                        last_pos = cur_pos;
                        cur_pos += 2;
                        while (cur_pos < content.Length)
                        {
                            if (content[cur_pos] == '*' && cur_pos + 1 < content.Length && content[cur_pos + 1] == '/')
                            {
                                cur_pos += 2;
                                break;
                            }
                            cur_pos++;
                        }
                        ret.Add(new Token(last_pos, cur_pos, Lexer.TokenColor.Green));
                        last_pos = cur_pos;
                        continue;
                    }
                }
                //字符串
                else if (content[cur_pos] == '\"')
                {
                    ret.Add(new Token(last_pos, cur_pos,
                    Lexer.get_token_info(content.Substring(last_pos, cur_pos - last_pos))));
                    last_pos = cur_pos;
                    cur_pos++;
                    while (true)
                    {
                        if (cur_pos >= content.Length)
                            break;
                        if (cur_pos < content.Length && content[cur_pos] == '\\')
                        {
                            cur_pos += 2;
                        }
                        if (cur_pos < content.Length && content[cur_pos] == '\"')
                        {
                            cur_pos++;
                            break;
                        }
                        cur_pos++;
                    }
                    ret.Add(new Token(last_pos, cur_pos, Lexer.TokenColor.DarkRed));
                    last_pos = cur_pos;
                    continue;
                }
                //分隔符
                else if (Lexer.is_split_char(content[cur_pos]))
                {
                    ret.Add(new Token(last_pos, cur_pos,
                     Lexer.get_token_info(content.Substring(last_pos, cur_pos - last_pos))));
                    cur_pos++;
                    last_pos = cur_pos;
                    continue;
                }
                cur_pos++;
            }
            if (last_pos != cur_pos)
            {
                ret.Add(new Token(last_pos, cur_pos,Lexer.get_token_info(content.Substring(last_pos, cur_pos - last_pos))));
            }
            return ret;

        }
        private string content;
    }

}

