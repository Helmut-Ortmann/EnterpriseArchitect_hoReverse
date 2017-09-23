using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace hoReverse.Reverse
{//----------------------------------------------------------------------------
 // Overwrite IsInputKey
 //----------------------------------------------------------------------------
 // The KeyDown event only triggered at the standard TextBox or MaskedTextBox by "normal" input keys, 
 // not ENTER or TAB and so on.
    public class EnterTextBox : TextBox
    {
        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Return)
                return true;
            return base.IsInputKey(keyData);
        }

    }
}
