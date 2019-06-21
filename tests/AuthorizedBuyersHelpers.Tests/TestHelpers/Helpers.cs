using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace System {

    public static class ActionEnumerableExtensions {

        public static void Run(this IEnumerable<Action> actions, int parallelPerAction = 1) {
            Debug.Assert(actions != null);
            Debug.Assert(parallelPerAction >= 1);

            foreach (var action in actions) {
                if (parallelPerAction == 1) {
                    action();
                    continue;
                }

                using (var reset = new ManualResetEventSlim()) {
                    Parallel.For(0, parallelPerAction, _ => {
                        new Thread(() => {
                            action();
                            reset.Set();
                        }).Start();
                    });
                    reset.Wait();
                }
            }
        }
    }
}
