# <a name="readme"></a>notifications

Notifications are a flexible, global pub/sub message system.

## Install

From your unity project folder:

```bash
npm init --force && \
npm install beatthat/notifications
```

The package and all its dependencies will be installed under Assets/Plugins/packages.

In case it helps, a quick video of the above: https://youtu.be/Uss_yOiLNw8

## USAGE

If you're using this notifications package, you probably encountered it through other `beatthat` unity3d packages that leverage notifications and also simplify their use. Will give some examples of those packages further down, but for now if you were using Notifications without any other support it would look like this:

```csharp
using BeatThat.Notifications;

public static class StatusNotifications
{
    /// Notifications are identified by string types
    /// Usually, you want to define those types as constants somewhere
    public const string STATUS_UPDATED = "STATUS_UPDATED";
}

public class StatusObserver : MonoBehavior
{
    void Start()
    {
        NotificationBus.Add<string>(StatusNotifications.STATUS_UPDATED, this.OnStatusUpdated);
    }

    void OnStatusUpdated(string newStatus)
    {
        Debug.Log("got new status: " + newStatus);
    }
}

public class StatusPublisher : MonoBehavior
{
    public string status;

    public void SetStatus(string newStatus) 
    {
        this.status = d;
        NotificationsBus.Send(StatusNotifications.STATUS_UPDATED, newStatus)
    }
}
```

The example above is very contrived but the main idea is that `StatusObserver` can get status updates without needing to know anything about `StatusPublisher`. Instead they both depend on shared notification type.

## A Practical Example: State Stores

The example above is really a simplication of State Stores, which are a common use case for notificitions. The basic is to have a global singleton that manages some state item and then observers of that state that can both access the state value and subscribe to on-update notifications.

A more usable version of the status-update example can be built using the [state-stores](https://github.com/beatthat/state-stores) package (in concert with a few other packages I will detail below)

```csharp
/// pretend we have a slightly more complex 
/// StatusData struct for our state
public struct StatusData
{
    public string status;
    public bolean isHappy;
}

using BeatThat.Service;
using BeatThat.StateStores;
// register this singleton as implementation 
// of interface HasState<StatusData> (see beatthat/services below)
[RegisterService(HasState<StatusData>)]
public class StatusStore : StateStore<StatusData> 
{
    // Will expose the StatusData state as property `stateData`
    // Not shown how here, but just assume that state can be updated
}


using BeatThat.Controllers;
using BeatThat.DependencyInjection;
using BeatThat.StateStores;
public class StatusObserver : Controller 
/// extending Controller here mainly gives support for dependency-injection 
/// and simplified notificaton binding
{
    override protected void GoController() // called after dependencies injected
    {
        /// Bind is a wrapper for NotificationBus.Add 
        /// that handles cleanup, i.e. it makes sure 
        /// the registered callback is removed when this controller goes away
        Bind(State<StatusData>.UPDATED, this.OnStatusUpdated);
    }

    void OnStatusUpdated()
    {
        StatusData s = this.statusStore.stateData; // t
        Debug.Log("got new status: " + s.status + " with isHappy " + s.isHappy);
    }

    // dependency injection will set this property to our singleton service
    // based upon matching the registered interface HasState<StatusData>
    [Inject] HasState<StatusData> statusStore { get; set; }
}

```

For more details on the packages used above see:


#### The [service](https://github.com/beatthat/service) package manages a container of global singleton services

The `[RegisterService]` attribute above triggers the creation of a global singleton registered for lookup by interface `HasState<StatusData>`


#### The [dependency-injection](https://github.com/beatthat/dependency-injection) package assigns references to service singletons

The `[Inject]` causes that property to be assigned with the registered `HasState<StatusData>` singleton

#### The [controllers](https://github.com/beatthat/controllers) package simplifies use of dependency injection and notifications

Extending `Controller` gives out-of-box support for Dependency Injection and simplified notificaton binding. It isn't necessary to extend controller though. For example, you could alternatively enable dependency injection on a plain MonoBehavior like this:

```csharp
void Start()
{
    // Something needs to call DependencyInjection.InjectDependencies.
    // The Controller base class would have done this for you
    BeatThat.DependencyInjection.InjectDependencies.On(this);
}
```

#### Making sure to cleanup/unregister notification listeners

If you're listening for notifications from a class that doesn't live forever0--e.g. a screen--it's important to always unregister any listeners attached to the `NotificationBus`. For example, if you have a screen that's listening for those `StatusData` notifications above, and then the user exits/destroys that screen, it will cause errors and memory leaks if the (now zombie/destroyed) screen continues receiving notifications. 

This is another reason we use that `Controller` base class above. Because it has a `Bind(string notificationType, System.Action callback)` function that takes care of that cleanup for you.

If you were making a screen from raw MonoBehavior and wanted to implement register + cleanup of noticiations properly, it would look like this:

```csharp
using BeatThat.Notifications;
class MyScreen : MonoBehavior
{
    private NotificationBinding binding;
    void OnEnable() {
        this.binding = NotificationBus.Add("somenotification", this.OnNotification);
    }

    void OnNotification() {
        // do whatever
    }

    void OnDisable() {
        if(this.binding != null) {
            this.binding.Unbind();
            this.binding = null;
        }
    }
}
```

...again for comparison, if we used the `Controller` base class the same thing would look like this:
```csharp
using BeatThat.Controllers;
using BeatThat.Notifications;
class MyScreen : Controller
{
    override protected void OnGoController() {
        // we don't need to worry about cleaning up,
        // controller does that for us
        Bind("somenotification", this.OnNotification);
    }

    void OnNotification() {
        // do whatever
    }
}
```


