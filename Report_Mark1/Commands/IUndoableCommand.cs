using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_Mark1.Commands
{
    public interface IUndoableCommand
    {
        void Execute();
        void Undo();
    }
}
