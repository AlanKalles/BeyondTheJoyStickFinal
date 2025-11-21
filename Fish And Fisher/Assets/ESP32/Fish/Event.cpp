#include <functional>
#include <unordered_map>
#include <memory>

template<typename... Args>
class Event {
public:
    using Handler = std::function<void(Args...)>;
    using HandlerId = uint64_t;

    class EventSubscription {
    public:
        EventSubscription(Event* event, HandlerId id) : event(event), id(id) {}
        ~EventSubscription() { if (event) event->unsubscribe(id); }

        // 可手动取消
        void unsubscribe() { if(event){ event->unsubscribe(id); event=nullptr; } }

    private:
        Event* event;
        HandlerId id;
    };

    std::shared_ptr<EventSubscription> subscribe(Handler handler) {
        HandlerId id = nextId++;
        handlers[id] = std::move(handler);
        return std::make_shared<EventSubscription>(this, id);
    }

    bool unsubscribe(HandlerId id) {
        return handlers.erase(id) > 0;
    }

    void notify(Args... args) {
        for (auto &pair : handlers) {
            pair.second(args...);
        }
    }

private:
    HandlerId nextId = 1;
    std::unordered_map<HandlerId, Handler> handlers;
};
