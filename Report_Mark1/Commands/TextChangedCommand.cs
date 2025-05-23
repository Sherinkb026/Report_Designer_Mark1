using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Report_Mark1.Commands
{
    public class TextChangeCommand : IUndoableCommand
    {
        private TextBlock _target;
        private string _oldText;
        private string _newText;

        public TextChangeCommand(TextBlock target, string oldText, string newText)
        {
            _target = target;
            _oldText = oldText;
            _newText = newText;
        }

        public void Execute()
        {
            _target.Text = _newText;
        }

        public void Undo()
        {
            _target.Text = _oldText;
        }
    }

}
