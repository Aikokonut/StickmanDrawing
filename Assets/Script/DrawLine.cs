using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Stickman
{
    [RequireComponent(typeof(LineRenderer))]
    public class DrawLine : MonoBehaviour
    {
        [Header("Drawing")]
        [SerializeField] float minPointDistance = 0.1f;
        [SerializeField] float lineWidth = 0.1f;
        [SerializeField] float startGrabRadius = 1.0f;

        [Header("References")]
        [SerializeField] LineRenderer lineRenderer;
        [SerializeField] Transform player;
        [SerializeField] Camera worldCamera;

        public event Action<IReadOnlyList<Vector2>> OnPathDrawn;
        public Func<IReadOnlyList<Vector2>, int> PathFinalizer;

        readonly List<Vector2> points = new List<Vector2>(1024);
        bool isDrawing;
        bool wasPressed;
        Vector2 lastPointer;

        public IReadOnlyList<Vector2> Points => points;

        void Awake()
        {
            if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
            if (worldCamera == null) worldCamera = Camera.main;

            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = lineRenderer.endWidth = lineWidth;
            lineRenderer.positionCount = 0;
        }

        void OnValidate()
        {
            minPointDistance = Mathf.Max(0.001f, minPointDistance);
            if (lineRenderer != null) lineRenderer.startWidth = lineRenderer.endWidth = lineWidth;
        }

        void Update()
        {
            bool pressed = IsPressed();
            Vector2 world = pressed ? GetPointerWorld() : default;

            if (pressed && !wasPressed) Begin(world);
            else if (pressed && isDrawing) Continue(world);
            else if (!pressed && wasPressed && isDrawing) End();

            wasPressed = pressed;
        }

        void Begin(Vector2 world)
        {
            if (player != null)
            {
                Vector2 playerPos = player.position;
                if ((world - playerPos).sqrMagnitude > startGrabRadius * startGrabRadius)
                {
                    isDrawing = false;
                    return;
                }
            }

            Clear();
            isDrawing = true;
            lastPointer = world;

            Vector2 start = (player != null) ? (Vector2)player.position : world;
            AddPoint(start);

            if (start != world) AppendSubdivided(start, world);
        }

        void Continue(Vector2 world)
        {
            if (points.Count == 0) return;

            Vector2 last = points[points.Count - 1];
            if ((world - last).sqrMagnitude < minPointDistance * minPointDistance) return;

            AppendSubdivided(last, world);
            lastPointer = world;
        }

        void End()
        {
            isDrawing = false;

            if (points.Count < 2) { Clear(); return; }

            int keep = PathFinalizer != null ? PathFinalizer(points) : points.Count;
            keep = Mathf.Clamp(keep, 0, points.Count);

            if (keep < 2) { Clear(); return; }

            if (keep < points.Count)
            {
                points.RemoveRange(keep, points.Count - keep);
                lineRenderer.positionCount = keep;
            }

            OnPathDrawn?.Invoke(points);
        }

        public void Clear()
        {
            points.Clear();
            lineRenderer.positionCount = 0;
        }

        void AppendSubdivided(Vector2 from, Vector2 to)
        {
            float dist = Vector2.Distance(from, to);
            if (dist <= 0f) return;

            int steps = Mathf.Max(1, Mathf.CeilToInt(dist / minPointDistance));
            float inv = 1f / steps;
            for (int i = 1; i <= steps; i++)
                AddPoint(Vector2.Lerp(from, to, i * inv));
        }

        void AddPoint(Vector2 world)
        {
            points.Add(world);
            int idx = points.Count - 1;
            if (lineRenderer.positionCount < points.Count)
                lineRenderer.positionCount = points.Count;
            lineRenderer.SetPosition(idx, new Vector3(world.x, world.y, 0f));
        }

        bool IsPressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) return true;
            if (Mouse.current != null && Mouse.current.leftButton.isPressed) return true;
            return false;
#else
            if (Input.touchCount > 0)
            {
                TouchPhase phase = Input.GetTouch(0).phase;
                return phase != TouchPhase.Ended && phase != TouchPhase.Canceled;
            }
            return Input.GetMouseButton(0);
#endif
        }

        Vector2 GetPointerScreen()
        {
#if ENABLE_INPUT_SYSTEM
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
                return Touchscreen.current.primaryTouch.position.ReadValue();
            if (Mouse.current != null) return Mouse.current.position.ReadValue();
            return Vector2.zero;
#else
            if (Input.touchCount > 0) return Input.GetTouch(0).position;
            return Input.mousePosition;
#endif
        }

        Vector2 GetPointerWorld()
        {
            if (worldCamera == null) worldCamera = Camera.main;
            if (worldCamera == null) return Vector2.zero;
            Vector3 screen = GetPointerScreen();
            screen.z = -worldCamera.transform.position.z;
            Vector3 w = worldCamera.ScreenToWorldPoint(screen);
            return new Vector2(w.x, w.y);
        }
    }
}
