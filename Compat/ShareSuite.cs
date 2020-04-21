using System.Runtime.CompilerServices;

namespace ThinkInvisible.ClassicItems {
    static class Compat_ShareSuite {
        //taken from https://github.com/harbingerofme/DebugToolkit/blob/master/Code/DT-Commands/Money.cs
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static void GiveMoney(uint amount) {
            ShareSuite.MoneySharingHooks.AddMoneyExternal((int) amount);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static bool MoneySharing() {
            if (ShareSuite.ShareSuite.MoneyIsShared.Value)
                return true;
            return false;
        }
    }
}
