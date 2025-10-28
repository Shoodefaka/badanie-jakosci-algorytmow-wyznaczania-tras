using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour {
    public GameObject[] targets;
    public GameObject target;
    TextManager textManager;
    float speed;
    float maxSpeed;
    float slowedSpeed;
    Vector3[] path;
    int targetIndex;
    public bool useJPS = false;

    void Awake() {
        textManager = FindFirstObjectByType<TextManager>();
        targets = GameObject.FindGameObjectsWithTag("Shop");
        target = targets[UnityEngine.Random.Range(0, targets.Length)];
        speed = Random.Range(1f, 2.5f);
        maxSpeed = speed;
        slowedSpeed = speed/2;
    }

    public void StartRequest(bool useJps) {
        if (useJps) {
            PathRequestManager.RequestPathJPS(transform.position, target.transform.position, OnPathFound);
        }
        else {
            PathRequestManager.RequestPath(transform.position, target.transform.position, OnPathFound);
        }
    }

    void OnTriggerEnter2D(Collider2D collision) {
        speed = slowedSpeed;
    }

    void OnTriggerExit2D(Collider2D collision) {
        speed = maxSpeed;
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful) {
        if (pathSuccessful) {
            path = newPath;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator FollowPath() {
        Vector3 currentWaypoint = path[0];
        while (true) {
            if (transform.position == currentWaypoint) {
                targetIndex++;
                if (targetIndex >= path.Length) {
                    textManager.AddScore();
                    Destroy(gameObject);
                    yield break;
                }
                currentWaypoint = path[targetIndex];
            }

            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed*Time.deltaTime);
            yield return null;
        }
    }

    //// Drawing paths for debugging
    //public void OnDrawGizmos() {
    //    if (path != null && path.Length > 0) {
    //        if (useJPS) {
    //            Gizmos.color = Color.green;
    //            for (int i = 0; i < path.Length; i++) {
    //                Gizmos.DrawSphere(path[i], 0.2f);
    //            }

    //            Gizmos.color = Color.green;
    //            Vector3 prev = transform.position;
    //            for (int i = 0; i < path.Length; i++) {
    //                Gizmos.DrawLine(prev, path[i]);
    //                prev = path[i];
    //            }
    //        }
    //        else {
    //            Gizmos.color = Color.cyan;
    //            Vector3 prevPos = transform.position;
    //            for (int i = targetIndex; i < path.Length; i++) {
    //                Gizmos.DrawLine(prevPos, path[i]);
    //                Gizmos.DrawCube(path[i], new Vector3(0.15f, 0.15f, 0.15f));
    //                prevPos = path[i];
    //            }
    //        }
    //    }
    //}
}


