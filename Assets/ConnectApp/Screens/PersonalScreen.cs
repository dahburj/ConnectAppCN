using System.Collections.Generic;
using ConnectApp.Components;
using ConnectApp.Constants;
using ConnectApp.Main;
using ConnectApp.Models.ActionModel;
using ConnectApp.Models.State;
using ConnectApp.Models.ViewModel;
using ConnectApp.Plugins;
using ConnectApp.redux.actions;
using ConnectApp.Utils;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    public class PersonalScreenConnector : StatelessWidget {
        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, PersonalScreenViewModel>(
                converter: state => new PersonalScreenViewModel {
                    isLoggedIn = state.loginState.isLoggedIn,
                    user = state.loginState.loginInfo,
                    userDict = state.userState.userDict,
                    userLicenseDict = state.userState.userLicenseDict,
                    currentTabBarIndex = state.tabBarState.currentTabIndex
                },
                builder: (context1, viewModel, dispatcher) => {
                    return new PersonalScreen(
                        viewModel: viewModel,
                        new PersonalScreenActionModel {
                            mainRouterPushTo = routeName => dispatcher.dispatch(new MainNavigatorPushToAction {
                                routeName = routeName
                            }),
                            pushToUserDetail = userId => dispatcher.dispatch(new MainNavigatorPushToUserDetailAction {
                                userId = userId
                            })
                        }
                    );
                }
            );
        }
    }

    public class PersonalScreen : StatefulWidget {
        public PersonalScreen(
            PersonalScreenViewModel viewModel = null,
            PersonalScreenActionModel actionModel = null,
            Key key = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly PersonalScreenViewModel viewModel;
        public readonly PersonalScreenActionModel actionModel;

        public override State createState() {
            return new _PersonalScreenState();
        }
    }

    public class _PersonalScreenState : State<PersonalScreen>, RouteAware {
        string _loginSubId;
        string _logoutSubId;

        public override void initState() {
            base.initState();
            StatusBarManager.statusBarStyle(UserInfoManager.isLogin());
            this._loginSubId = EventBus.subscribe(sName: EventBusConstant.login_success,
                _ => { StatusBarManager.statusBarStyle(true); });
            this._logoutSubId = EventBus.subscribe(sName: EventBusConstant.logout_success,
                _ => { StatusBarManager.statusBarStyle(false); });
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            Router.routeObserve.subscribe(this, (PageRoute) ModalRoute.of(context: this.context));
        }

        public override void dispose() {
            Router.routeObserve.unsubscribe(this);
            EventBus.unSubscribe(sName: EventBusConstant.login_success, id: this._loginSubId);
            EventBus.unSubscribe(sName: EventBusConstant.logout_success, id: this._logoutSubId);
            base.dispose();
        }

        public override Widget build(BuildContext context) {
            return new Container(
                color: CColors.White,
                child: new Column(
                    children: new List<Widget> {
                        this.widget.viewModel.isLoggedIn
                            ? this._buildLoginInNavigationBar()
                            : this._buildNotLoginInNavigationBar(),
                        this.widget.viewModel.isLoggedIn
                            ? (Widget) new Container()
                            : new CustomDivider(
                                color: CColors.Separator2,
                                height: 1
                            ),
                        new Container(height: 16),
                        new Flexible(
                            child: new Column(
                                children: this._buildItems()
                            )
                        )
                    }
                )
            );
        }

        Widget _buildNotLoginInNavigationBar() {
            return new Container(
                color: CColors.White,
                padding: EdgeInsets.only(16, CCommonUtils.getSafeAreaTopPadding(context: this.context), bottom: 16),
                child: new Column(
                    mainAxisAlignment: MainAxisAlignment.end,
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: new List<Widget> {
                        _buildQrScanWidget(),
                        new Text("欢迎来到", style: CTextStyle.H2),
                        new Text("Unity Connect", style: CTextStyle.H2),
                        new Container(
                            margin: EdgeInsets.only(top: 16),
                            child: new CustomButton(
                                padding: EdgeInsets.zero,
                                onPressed: () =>
                                    this.widget.actionModel.mainRouterPushTo(obj: MainNavigatorRoutes.Login),
                                child: new Container(
                                    padding: EdgeInsets.symmetric(horizontal: 24, vertical: 8),
                                    decoration: new BoxDecoration(
                                        border: Border.all(color: CColors.PrimaryBlue),
                                        borderRadius: BorderRadius.all(20)
                                    ),
                                    child: new Text(
                                        "登录/注册",
                                        style: CTextStyle.PLargeMediumBlue
                                    )
                                )
                            )
                        )
                    }
                )
            );
        }

        Widget _buildLoginInNavigationBar() {
            var user = this.widget.viewModel.userDict[key: this.widget.viewModel.user.userId];
            Widget titleWidget;
            if (user.title != null && user.title.isNotEmpty()) {
                titleWidget = new Text(
                    data: user.title,
                    style: new TextStyle(
                        fontSize: 14,
                        fontFamily: "Roboto-Regular",
                        color: CColors.BgGrey
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis
                );
            }
            else {
                titleWidget = new Container();
            }

            var content = new Container(
                height: 184 + CCommonUtils.getSafeAreaTopPadding(context: this.context),
                padding: EdgeInsets.only(16, CCommonUtils.getSafeAreaTopPadding(context: this.context), bottom: 16),
                child: new Column(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: new List<Widget> {
                        _buildQrScanWidget(),
                        new Row(
                            children: new List<Widget> {
                                new Container(
                                    margin: EdgeInsets.only(right: 12),
                                    child: Avatar.User(
                                        user: user,
                                        64,
                                        true
                                    )
                                ),
                                new Expanded(
                                    child: new Column(
                                        mainAxisAlignment: MainAxisAlignment.center,
                                        crossAxisAlignment: CrossAxisAlignment.start,
                                        children: new List<Widget> {
                                            new Row(
                                                crossAxisAlignment: CrossAxisAlignment.start,
                                                children: new List<Widget> {
                                                    new Flexible(
                                                        child: new Text(
                                                            user.fullName ?? user.name,
                                                            style: CTextStyle.H4White.merge(new TextStyle(height: 1)),
                                                            maxLines: 1,
                                                            overflow: TextOverflow.ellipsis
                                                        )
                                                    ),
                                                    CImageUtils.GenBadgeImage(
                                                        badges: user.badges,
                                                        CCommonUtils.GetUserLicense(
                                                            userId: user.id,
                                                            userLicenseMap: this.widget.viewModel.userLicenseDict
                                                        ),
                                                        EdgeInsets.only(4, 6)
                                                    )
                                                }
                                            ),
                                            titleWidget
                                        }
                                    )
                                ),
                                new Container(
                                    padding: EdgeInsets.only(12, right: 16),
                                    child: new Icon(
                                        icon: Icons.chevron_right,
                                        size: 24,
                                        color: CColors.LightBlueGrey
                                    )
                                )
                            }
                        )
                    }
                )
            );
            return new GestureDetector(
                onTap: () => this.widget.actionModel.pushToUserDetail(obj: user.id),
                child: new Stack(
                    children: new List<Widget> {
                        Positioned.fill(new Stack(
                            children: new List<Widget> {
                                new Stack(
                                    children: new List<Widget> {
                                        new Container(color: new Color(0xFF212121)),
                                        new Positioned(
                                            top: 30,
                                            right: 30,
                                            child: new Icon(Icons.UnityLogo, size: 210, color: new Color(0xFF2b2b2b)))
                                    })
                            })),
                        content
                    })
            );
        }

        static Widget _buildQrScanWidget() {
            return new Row(
                mainAxisAlignment: MainAxisAlignment.end,
                children: new List<Widget> {
                    new CustomButton(
                        padding: EdgeInsets.only(16, 16, 20, 16),
                        onPressed: QRScanPlugin.PushToQRScan,
                        child: new Icon(
                            icon: Icons.qr_scan,
                            size: 28,
                            color: CColors.LightBlueGrey
                        )
                    )
                }
            );
        }

        List<Widget> _buildItems() {
            return new List<Widget> {
                new CustomListTile(
                    new Icon(icon: Icons.book, size: 24, color: CColors.TextBody2),
                    "我的收藏",
                    trailing: CustomListTileConstant.defaultTrailing,
                    onTap: () => {
                        var routeName = this.widget.viewModel.isLoggedIn
                            ? MainNavigatorRoutes.MyFavorite
                            : MainNavigatorRoutes.Login;
                        this.widget.actionModel.mainRouterPushTo(obj: routeName);
                    }
                ),
                new CustomListTile(
                    new Icon(icon: Icons.outline_event, size: 24, color: CColors.TextBody2),
                    "我的活动",
                    trailing: CustomListTileConstant.defaultTrailing,
                    onTap: () => {
                        var routeName = this.widget.viewModel.isLoggedIn
                            ? MainNavigatorRoutes.MyEvent
                            : MainNavigatorRoutes.Login;
                        this.widget.actionModel.mainRouterPushTo(obj: routeName);
                    }
                ),
                new CustomListTile(
                    new Icon(icon: Icons.eye, size: 24, color: CColors.TextBody2),
                    "浏览历史",
                    trailing: CustomListTileConstant.defaultTrailing,
                    onTap: () => this.widget.actionModel.mainRouterPushTo(obj: MainNavigatorRoutes.History)
                ),
                new CustomListTile(
                    new Icon(icon: Icons.settings, size: 24, color: CColors.TextBody2),
                    "设置",
                    trailing: CustomListTileConstant.defaultTrailing,
                    onTap: () => this.widget.actionModel.mainRouterPushTo(obj: MainNavigatorRoutes.Setting)
                )
            };
        }

        public void didPopNext() {
            if (this.widget.viewModel.currentTabBarIndex == 3) {
                StatusBarManager.statusBarStyle(UserInfoManager.isLogin());
            }
        }

        public void didPush() {
        }

        public void didPop() {
        }

        public void didPushNext() {
        }
    }
}