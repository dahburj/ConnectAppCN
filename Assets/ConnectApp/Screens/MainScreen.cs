using System.Collections.Generic;
using ConnectApp.Components;
using ConnectApp.Constants;
using ConnectApp.Main;
using ConnectApp.Plugins;
using ConnectApp.redux;
using ConnectApp.redux.actions;
using ConnectApp.Utils;
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    public class MainScreen : StatelessWidget {
        public override Widget build(BuildContext context) {
            var child = new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    top: false,
                    bottom: false,
                    child: new CustomTabBarConnector(
                        new List<Widget> {
                            new ArticlesScreenConnector(),
                            new EventsScreen(),
                            new MessengerScreenConnector(),
                            new PersonalScreenConnector()
                        },
                        new List<CustomTabBarItem> {
                            new CustomTabBarItem(
                                0,
                                Icons.UnityTabIcon,
                                Icons.UnityTabIcon,
                                "首页"
                            ),
                            new CustomTabBarItem(
                                1,
                                Icons.outline_event,
                                Icons.eventIcon,
                                "活动"
                            ),
                            new CustomTabBarItem(
                                2,
                                Icons.outline_question_answer,
                                Icons.question_answer,
                                "群聊"
                            ),
                            new CustomTabBarItem(
                                3,
                                Icons.mood,
                                Icons.mood,
                                "我的"
                            )
                        },
                        backgroundColor: CColors.TabBarBg,
                        (fromIndex, toIndex) => {
                            AnalyticsManager.ClickHomeTab(fromIndex: fromIndex, toIndex: toIndex);

                            if (toIndex != 2 || StoreProvider.store.getState().loginState.isLoggedIn) {
                                StatusBarManager.statusBarStyle(toIndex == 3 && UserInfoManager.isLogin());
                                StoreProvider.store.dispatcher.dispatch(new SwitchTabBarIndexAction {index = toIndex});
                                JPushPlugin.showPushAlert(toIndex != 2);
                                PreferencesManager.updateTabIndex(toIndex);
                                return true;
                            }

                            Router.navigator.pushNamed(routeName: MainNavigatorRoutes.Login);
                            return false;
                        },
                        initialTabIndex: PreferencesManager.initTabIndex()
                    )
                )
            );
            return new VersionUpdater(
                child: child
            );
        }
    }
}