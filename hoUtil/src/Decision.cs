

// ReSharper disable once CheckNamespace
namespace hoReverse.hoUtils
{
    public static class Decision
    {
        /// <summary>
        /// Get the outgoing guards as aggregated string like '','yes','yesno',...
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="elDecision"></param>
        /// <returns></returns>
        public static string GetDecisionOutgoingGuards(EA.Repository rep, EA.Element elDecision)
        {
            string guards = "";
            foreach (EA.Connector con in elDecision.Connectors)
            {
                if (con != null)
                {
                    // Check if connector starts from Decision
                    EA.Element sourceEl = rep.GetElementByID(con.ClientID);
                    if (sourceEl.ElementID == elDecision.ElementID)
                    {
                        // Connector from Decision to.....
                        if (con.TransitionGuard.ToLower().Trim() == "yes" || con.TransitionGuard.ToLower().Trim() == "no")
                            guards = guards + con.TransitionGuard.ToLower().Trim();
                    }
                }
            }
            // no outgoing decision found
            return guards;

        }
    }
}
