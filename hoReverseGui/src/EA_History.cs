using System.Collections.ObjectModel;
using EA;
using hoReverse.Settings;
using hoReverse.Reverse;

// ReSharper disable once CheckNamespace
namespace hoReverse.HistoryList 
{
    
    public class EaHistoryListEntry
    {
        public string BookmarkName { get; } = "";

        public string EaObjectTypeName { get; set; } = "";

        public EA.ObjectType EaTyp { get; }

        public string Guid { get; }

        public EaHistoryListEntry(EA.ObjectType eaType, string guid)
        {
            EaTyp = eaType;
            Guid = guid;
            //this.update();
        }
        public EaHistoryListEntry(EA.ObjectType eaType, string guid, string bookmarkName)
        {
            EaTyp = eaType;
            Guid = guid;
            this.BookmarkName = bookmarkName;
            //this.update();
        }
        public void Update() {
            switch (EaTyp)
            {
                case EA.ObjectType.otElement:
                    EaObjectTypeName = "Element";
                    break;
                case EA.ObjectType.otPackage:
                    EaObjectTypeName = "Package";
                    break;
                case EA.ObjectType.otDiagram:
                    EaObjectTypeName = "Diagram";
                    break;

            }
        }

    }
    public class EaHistoryList
    {
        private readonly string _historyType;
        protected readonly ObservableCollection<EaHistoryListEntry> L;
        //private ObservableCollection<int> m_l = null;
        private readonly EA.Repository _rep;

        private readonly AddinSettings _settings;

        // diagram history with internal id of diagrams
        protected int LPosition;
        //private List<int> m_bookmark = null; 
        protected EaHistoryList(string historyType, EA.Repository rep, AddinSettings settings)
        {
            _historyType = historyType;
            _rep = rep;
            _settings = settings;
            //m_l = new ObservableCollection<EaHistoryListEntry>();
            L = new ObservableCollection<EaHistoryListEntry>();
            L = settings.GetHistory(historyType, _rep.ProjectGUID);

            if (L.Count > 0) LPosition = L.Count - 1;
            else LPosition = -1; // no bookmark history available
            this.RemoveInvalidIds();
        }
        private void RemoveInvalidIds()
        {
            for (int i = L.Count - 1; i >= 0; i--)
            {
                try
                {
                    switch (L[i].EaTyp) {
                        case ObjectType.otDiagram:
                            EA.Diagram dia = (EA.Diagram)_rep.GetDiagramByGuid(L[i].Guid);
                            if (dia == null)
                            {
                                L.RemoveAt(i);
                                continue;
                            }
                            UpdateDiagram(i, dia);
                            break;
                        case ObjectType.otPackage:
                            EA.Package pkg = _rep.GetPackageByGuid(L[i].Guid);
                            if (pkg == null)
                            {
                                L.RemoveAt(i);
                                continue;
                            }
                            UpdatePackage(i, pkg);
                            break;

                        case ObjectType.otElement:
                            EA.Element el = _rep.GetElementByGuid(L[i].Guid);
                            if (el == null)
                            {
                                L.RemoveAt(i);
                                continue;
                            }
                            UpdateElement(i, el);
                            break;
                }
                }
                catch //(Exception e)
                {
                    L.RemoveAt(i);
                }
            }
            LPosition = L.Count - 1;

        }

        private void UpdateElement(int i, EA.Element el)
        {
            L[i].EaObjectTypeName = "Element";
            if ("Component, Activity, Interface, Class".Contains(el.Type))
            {
            }
            else
            {
            }
        }

        private void UpdatePackage(int i, EA.Package pkg)
        {
            L[i].EaObjectTypeName = "Package";
        }

        private void UpdateDiagram(int i, EA.Diagram dia)
        {
            L[i].EaObjectTypeName = "Diagram";
        }

        // ReSharper disable once UnusedMember.Local
        public bool SetHistory (string listType, string guid, ObservableCollection<EaHistoryListEntry> l) {
            _settings.SetHistory(listType, guid, l);
            return true;
        }
        //
        private int AddEntry(EA.ObjectType ot, string guid)
        {
            if (L.Count > 0)
            {
                if (L[0].Guid == guid) return -1;// Entry exists already
            }
            L.Add(new EaHistoryListEntry(ot, guid));
            if (L.Count > 50)
            {
                L.RemoveAt(0);
                if (LPosition > -1) LPosition = LPosition - 1;
            }
            // set settings of project
            //m_settings.setHistory(m_historyType, m_rep.ProjectGUID, m_l);
            return L.Count - 1;

        }

        public void Remove(EA.ObjectType ot, string guid)
        {
            if (ot.Equals(EA.ObjectType.otDiagram))
            {
                EA.Diagram dia = (EA.Diagram)_rep.GetDiagramByGuid(guid);
                if (dia != null)
                {
                    int index = this.FindIndex(dia.DiagramGUID);
                    if (index > -1)
                    {
                        L.RemoveAt(index);
                        if (index <= LPosition) LPosition = LPosition - 1;
                    }
                }
            // settings of project
            _settings.SetHistory(_historyType, _rep.ProjectGUID, L);
            }
        }
        public void RemoveAll()
        {
            L.Clear();
            _settings.ResetHistory(_historyType);
        }

        // diagram viewed, add to history
        public virtual void  Add(EA.ObjectType ot, string guid)
        {
            if (ot.Equals(EA.ObjectType.otDiagram))
            {
                EA.Diagram dia = (EA.Diagram)_rep.GetDiagramByGuid(guid);
                    if (dia != null)
                    {
                        if (LPosition < 0 || L[LPosition].Guid != guid)
                        {
                        
                            int position = AddEntry(ot, guid);
                            if (position > -1)
                            { 
                                    UpdateDiagram(position, dia);
                                }
                            }
                    }
            }
            if (ot.Equals(EA.ObjectType.otPackage))
            {
                EA.Package pkg = _rep.GetPackageByGuid(guid);
                if (pkg != null)
                {
                    // not already defined as bookmark
                    if (this.FindIndex(pkg.PackageGUID) == -1)
                    {
                        int position = AddEntry(ot, guid);
                        if (position > -1)
                        {
                            UpdatePackage(position,pkg);
                        }

                    }

                }
            }
            if (ot.Equals(EA.ObjectType.otElement))
            {
                EA.Element el = _rep.GetElementByGuid(guid);
                if (el != null)
                {
                    // not already defined as bookmark
                    if (this.FindIndex(el.ElementGUID) == -1)
                    {
                        int position = AddEntry(ot, guid);
                        if (position > -1)
                        {
                            UpdateElement(position, el);

                        }

                    }

                }
            }
        }
        public void Back()
        {
            // go back in history
            LPosition = LPosition - 1;
            if (LPosition < 0) LPosition = L.Count -1;
            if (LPosition > -1) HoReverseGui.OpenDiagram(_rep, L[LPosition].Guid);
          
        }
        public int GetCount()
        {
            return L.Count;

        }
        public EaHistoryListEntry GetByIndex(int index)
        {
            return L[index];
        }
        public void Frwrd()
        {
            LPosition = LPosition + 1;
            if (LPosition > L.Count -1)// start from newest element
            {
                LPosition = 0;
            }
            if (L.Count > 0)// more than one element available
            {
                HoReverseGui.OpenDiagram(_rep, L[LPosition].Guid);
            }

            
            
        }
        public ObservableCollection<EaHistoryListEntry> GetAll()
        {
         //   removeInvalidIds();
              return L;
        }
        public EaHistoryListEntry GetLatest()
        {
            if (L.Count > 0)
            {
                return  L[L.Count - 1];
            }
            return null;
        }

        protected int FindIndex(string guid)
        {
            if (L.Count == 0) return -1;
            int index = -1;            
            foreach (EaHistoryListEntry unused in L)
            {
                index = index + 1;
                if (L[index].Guid == guid) return index;
            }
            return -1;
        }
    }
    public class EaBookmark: EaHistoryList
    {
        public EaBookmark(EA.Repository rep, AddinSettings settings) : base("bookmark", rep, settings)
        {
            
        }
        public override void Add(EA.ObjectType ot, string guid)
        {
            // bookmark not registered yet
            if (this.FindIndex(guid) == -1)
                base.Add(ot, guid);
        }
    }
    public class EaHistory: EaHistoryList
    {
        public EaHistory(EA.Repository rep, AddinSettings settings): base("history", rep, settings)
        {
            
        }
        public override void Add(EA.ObjectType ot, string guid) {
            // diagram is selected by add/return
            if (LPosition < 0)
            {
                base.Add(ot, guid);
            }
            else if (L[LPosition].Guid != guid)
            {
                base.Add(ot, guid);
            }
            else
            {
                  // do nothing
            }

        }
    }
}
