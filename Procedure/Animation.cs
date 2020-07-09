using Data.Arm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Procedure
{
    public class Animation
    {
        private LinkedList<KeyFrame> frames;
        private LinkedListNode<KeyFrame> curr;
        //private iterator<>

        //public delegate LinkedListNode<KeyFrame> NextDelegate();

        public Animation()
        {
            frames = new LinkedList<KeyFrame>();
            curr = frames.First;
        }

        public void Add(KeyFrame k)
        {
            frames.AddLast(k);
        }

        public LinkedListNode<KeyFrame> Next()
        {
            var node = curr;
            curr = curr.Next;
            return node;
        }

        public void Reset()
        {
            curr = frames.First;
        }
    }
}
