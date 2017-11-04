namespace hoReverse.Reverse.EaAddinShortcuts
{
    public class EaAddinShortcut
    {
        public string  keyText;
        public int keyPos;
        public string keySearchTooltip = "";
        public EaAddinShortcut(int pos, string Text, string toolTip)
        {
            keyPos = pos;
            keyText = Text;
            keySearchTooltip = toolTip;
        }
    }
    public class EaAddinShortcutSearch : EaAddinShortcut
    {
        public string keyType = "Search";
        public string keySearchName = "";
        public string keySearchTerm = "";
        public EaAddinShortcutSearch(int keyPos, string text, string searchName, string searchTerm, string toolTip)
            : base(keyPos, text, toolTip)
        {
            keySearchName = searchName;
            keySearchTerm = searchTerm;
        }
        public string HelpTextLog
        {
            get
            {
                if (keySearchName == "") return "";
                return ("Search\t\t:\t'" + keySearchName + "'" +
                      "\nSearchTerm\t:\t'" + keySearchTerm + "'" +
                      "\nDescription\t:\t" + keySearchTooltip);
            }
        }
    }
}
