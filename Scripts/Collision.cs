using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Collision : MonoBehaviour
{
    [SerializeField]
    protected string[] targetTags;

    protected Collider2D coll;

    protected List<Collider2D> colliders;
    public bool IsColliding { get {
        CheckColliders();
        return colliders != null && colliders.Count > 0;
    }}
    
    // The most recent object that entered the collision
    public Collider2D Collider { get {
        return IsColliding ? colliders[colliders.Count - 1] : null;
    }}
    // All of the objects currently inside the collision
    public List<Collider2D> AllColliders { get {
        CheckColliders();
        return colliders;
    }}

    protected virtual void OnEnable() {
        coll = GetComponent<Collider2D>();
        colliders = new List<Collider2D>();
    }
    protected virtual void OnDisable() {
        // If the object itself was disabled, clear the entire list
        colliders.Clear();
    }

    protected virtual void OnTriggerEnter2D(Collider2D other) {
        if (targetTags.Any(tag => other.CompareTag(tag)))
            colliders.Add(other);
    }
    protected void OnTriggerExit2D(Collider2D other) {
        if (targetTags.Any(tag => other.CompareTag(tag)))
            colliders.Remove(other);
    }

    void CheckColliders() {
        if (colliders == null) return;
        
        // If the object's own collider was disabled, clear the entire list
        if (!coll.enabled) {
            colliders.Clear();
            return;
        }

        // Remove any null or disabled colliders from the colliding list
        for (int c = colliders.Count - 1; c >= 0; c--) {
            if (colliders[c] == null || colliders[c].enabled == false)
                colliders.RemoveAt(c);
        }
    }

    // Returns the first collider in the list that passes a certain check function
    public Collider2D GetCollider(Func<Collider2D, bool> check) {
        CheckColliders();
        for (int c = colliders.Count - 1; c >= 0; c--) {
            if (check.Invoke(colliders[c]))
                return colliders[c];
        }

        return null;
    }
}
