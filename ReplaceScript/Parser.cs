using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ReplaceScript
{
    class Parser
    {
        State atualState;
        Stack<State> lastStateStack;

        string strNewFile = string.Empty;
        string strPHPAux = string.Empty;

        int line = 1;

        string before;
        string after;

        string filePath;


        public Parser(string filePath, string before, string after)
        {
            this.filePath = filePath;

            this.before = before;
            this.after = after;

            this.atualState = State.HTML;

            this.lastStateStack = new Stack<State>();
            this.lastStateStack.Push(State.HTML);
        }

        public string FilePath
        {
            get { return filePath; }
        }

        public void Replace()
        {
            List<Comparer> comparers = new List<Comparer>();

            Comparer beginPHPComparer = new Comparer("<?");
            beginPHPComparer.Found += BeginPHPFound;

            Comparer beginScriptComparer = new Comparer("<script");
            beginScriptComparer.Found += BeginScriptFound;

            Comparer endPHPComparer = new Comparer("?>");
            endPHPComparer.Found += EndPHPFound;

            Comparer endScriptComparer = new Comparer("</script>");
            endScriptComparer.Found += EndScriptFound;

            Comparer endOfLineComparer = new Comparer("\n");
            endOfLineComparer.Found += EndOfLineFound;

            comparers.Add(beginPHPComparer);
            comparers.Add(beginScriptComparer);
            comparers.Add(endPHPComparer);
            comparers.Add(endScriptComparer);
            comparers.Add(endOfLineComparer);

            string fileBuffer = File.ReadAllText(filePath);

            int atualPosition;

            for (atualPosition = 0; atualPosition < fileBuffer.Length; atualPosition++)
            {
                if (atualState != State.PHP)
                {
                    strNewFile += fileBuffer[atualPosition];
                }
                else
                {
                    strPHPAux += fileBuffer[atualPosition];
                }

                foreach (var comparer in comparers)
                {
                    comparer.Compare((char)fileBuffer[atualPosition]);
                }
            }

            if (strPHPAux != string.Empty)
            {
                EndPHPFound(null);
            }

            File.Delete(filePath);

            using (FileStream fs = new FileStream(filePath, FileMode.CreateNew))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(strNewFile);
                }
            }

            strNewFile = string.Empty;
            strPHPAux = string.Empty;

            line = 1;

            lastStateStack.Clear();
            lastStateStack.Push(State.HTML);
            atualState = State.HTML;
        }

        private void EndOfLineFound(Comparer obj)
        {
            line++;
        }

        private void EndScriptFound(Comparer obj)
        {
            atualState = lastStateStack.Pop();
        }

        private void EndPHPFound(Comparer obj)
        {
            atualState = lastStateStack.Pop();
            HandlePHP();
        }

        private void HandlePHP()
        {
            strPHPAux = Regex.Replace(strPHPAux, @"(?!\bif\b|\bfor\b|\bwhile\b|\bswitch\b|\btry\b|\bcatch\b)(\b" + before + @"\b)[\s\n\r]*(?=\(.*\))", after);

            strNewFile += strPHPAux;
            strPHPAux = string.Empty;
        }

        private void BeginScriptFound(Comparer obj)
        {
            if (atualState == State.PHP)
            {
                HandlePHP();
            }

            lastStateStack.Push(atualState);
            atualState = State.Script;
        }

        private void BeginPHPFound(Comparer obj)
        {
            lastStateStack.Push(atualState);
            atualState = State.PHP;
        }
    }
}
