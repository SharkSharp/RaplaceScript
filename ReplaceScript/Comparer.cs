using System;

namespace ReplaceScript
{
    class Comparer
    {
        public event Action<Comparer> Found;
        string toCompare;
        int compareIndex;

        public Comparer(string toCompare)
        {
            this.toCompare = toCompare;
            compareIndex = 0;
        }

        private void OnFoundCall()
        {
            if (Found != null)
            {
                Found(this);
            }
        }

        public string ToCompare
        {
            get { return toCompare; }
        }

        public void Compare(char atual)
        {
            if (atual == toCompare[compareIndex])
            {
                compareIndex++;
            }
            else
            {
                compareIndex = 0;
            }

            if (compareIndex == toCompare.Length)
            {
                OnFoundCall();
                compareIndex = 0;
            }
        }
    }
}
