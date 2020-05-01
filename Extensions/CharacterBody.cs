using RoR2;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ThinkInvisible.ClassicItems {
    public static class Ext_CharacterBody {
        private static Action<CharacterBody,BuffIndex,int> _setBuffCount;

        public static void SetBuffCount(this CharacterBody body, BuffIndex buffType, int newCount) {
            _setBuffCount(body, buffType, newCount);
        }

        static Ext_CharacterBody() {
            MethodInfo targetMethodInfo = typeof(CharacterBody).GetMethod("SetBuffCount", BindingFlags.NonPublic | BindingFlags.Instance);

            var bodyParam = Expression.Parameter(typeof(CharacterBody), "body");
            var buffIndexParam = Expression.Parameter(typeof(BuffIndex), "buffIndex");
            var countParam = Expression.Parameter(typeof(int), "count");

            _setBuffCount = Expression.Lambda<Action<CharacterBody,BuffIndex,int>>(
                Expression.Call(bodyParam, targetMethodInfo, buffIndexParam, countParam),
                bodyParam, buffIndexParam, countParam).Compile();
        }
    }
}
