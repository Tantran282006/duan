# Game Design Patterns Reference — Phố Nhỏ

> Mẫu thiết kế phần mềm cốt lõi (Patterns) tối ưu cho game Unity C#.

---

## 1. State Machine (Máy trạng thái hữu hạn)

Sử dụng cho trạng thái nhân vật (Idle, Walk, Interact, Busy) hoặc vòng đời đơn hàng:

```csharp
public interface IState
{
    void Enter();
    void Update();
    void FixedUpdate();
    void Exit();
}

public class StateMachine
{
    public IState CurrentState { get; private set; }

    public void ChangeState(IState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState?.Enter();
    }

    public void Update() => CurrentState?.Update();
    public void FixedUpdate() => CurrentState?.FixedUpdate();
}
```

---

## 2. Generic Object Pool

Tránh GC Alloc và giật lag khung hình khi spawn liên tục NPC, đồng xu Scoin, bóng thoại, hoặc hạt bụi bước chân:

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhoNho.Common
{
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _pool = new Queue<T>();

        public ObjectPool(T prefab, int initialSize, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;
            for (int i = 0; i < initialSize; i++)
            {
                T obj = GameObject.Instantiate(_prefab, _parent);
                obj.gameObject.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        public T Get()
        {
            T obj = _pool.Count > 0 ? _pool.Dequeue() : GameObject.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(true);
            return obj;
        }

        public void Return(T obj)
        {
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
}
```

---

## 3. Observer / Event Bus

Giao tiếp giữa Domain logic, UI và Network mà không tham chiếu trực tiếp (loose coupling):

```csharp
using System;

namespace PhoNho.Events
{
    public static class PhoNhoEvents
    {
        // Sự kiện kinh tế
        public static event Action<long> OnScoinBalanceChanged;
        public static void EmitScoinChanged(long newBalance) => OnScoinBalanceChanged?.Invoke(newBalance);

        // Sự kiện đơn hàng
        public static event Action<string> OnOrderCompleted;
        public static void EmitOrderCompleted(string orderId) => OnOrderCompleted?.Invoke(orderId);
    }
}
```
*Lưu ý*: Luôn hủy đăng ký sự kiện (`-=`) trong `OnDisable()` hoặc `OnDestroy()` để tránh rò rỉ bộ nhớ.
