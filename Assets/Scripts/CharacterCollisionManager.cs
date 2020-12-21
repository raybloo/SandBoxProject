using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCollisionManager : MonoBehaviour
{
    //public float radius = 3f; // compute collisiont with the colliders located inside a sphere of this radius
    //public float height
    
    public float tolerance = 0.001f;
    
    

    public bool collided = false;
    public float hardCollisionAngle = 130f;
    public bool hardCollided = false;


    public CapsuleCollider thisCollider;
    private Collider[] neighbours;
    private Vector3 start;
    private Vector3 end;

    //Displacement
    public float maxStepDistance = 1f; // discrete collision step length
    public int maxNeighbours = 4; // maximum amount of neighbours used to compute collision
    public int maxCollisionCount = 6; // maximum amount of collision taken into account (used for MoveS and MoveRB)

    private Vector3 currentMovement = Vector3.zero;
    private Vector3 probedMovement = Vector3.zero;
    private Vector3 correction = Vector3.zero;
    private Vector3[] dePenetrationVectors;
    private int dePenVectorCount;
    private Vector3 otherPosition = Vector3.zero;
    private Quaternion otherRotation = Quaternion.identity;

    //Rotation
    public int maxStepAngle = 15;
    private Quaternion currentRotation = Quaternion.identity;
    private Quaternion probedRotation = Quaternion.identity;

    //Debug
    public GameObject sphere1;
    public GameObject sphere2;

    void Start() {
        if(!thisCollider) {
            thisCollider = GetComponent<CapsuleCollider>();
        }
        neighbours = new Collider[maxNeighbours];
        dePenetrationVectors = new Vector3[maxCollisionCount];
        dePenVectorCount = 0;
        //probedRotations = new Quaternion[180/maxStepAngle];
    }
    
    public float MoveS(in Vector3 movement, out Vector3 corrected) {
        collided = false;
        if (!thisCollider) {
            corrected = movement;
            return 0f; // nothing to do without a Collider attached
        }

        float magnitude = movement.magnitude;
        float tests = 8;
        for (float i = 1f; i < 4; i++) {
            if (!(magnitude > i * maxStepDistance)) {
                tests = i;
                break;
            }
        }

        probedMovement = movement * (1f / tests);
        currentMovement = Vector3.zero;
        for (float i = 0f; i < tests; i++) {
            currentMovement += probedMovement;
            StepMoveS(ref currentMovement);
        }
        corrected = currentMovement;
        Debug.Log("moveS correction" + corrected + " tests :" + tests);
        return Vector3.Angle(movement, corrected);
    }
    
    //Check the movement and correction slides along colliders
    public void StepMoveS(ref Vector3 movement) {

        collided = false; 
        if (!thisCollider)
            return; // nothing to do without a Collider attached
        
        float radius = thisCollider.radius;
        float height = thisCollider.height - (2 * radius);

        start = thisCollider.transform.rotation * (thisCollider.center + (Vector3.down * height / 2)) + thisCollider.transform.position;
        end = thisCollider.transform.rotation * (thisCollider.center + (Vector3.up * height / 2)) + thisCollider.transform.position;

        int count;
        correction = Vector3.zero;
        dePenVectorCount = 0;

        for (int collisionCount = 0; collisionCount < maxCollisionCount; collisionCount++) {

            //Intersect collider with neighbours
            count = Physics.OverlapCapsuleNonAlloc(start + movement + correction, end + movement + correction, radius - tolerance, neighbours); 

            if(count > 0) {

                //Choose an intersecting neighbour
                var collider = neighbours[0];
                for (int i = 1; i < count; ++i) {
                    if (collider == thisCollider) {
                        collider = neighbours[i];
                    } else {
                        break;
                    }
                }

                if (collider != thisCollider) {

                    // Compute depen vector
                    otherPosition = collider.gameObject.transform.position;
                    otherRotation = collider.gameObject.transform.rotation;
                    bool overlapped = Physics.ComputePenetration(
                        thisCollider, thisCollider.transform.position + movement + correction, thisCollider.transform.rotation,
                        collider, otherPosition, otherRotation,
                        out Vector3 direction, out float distance
                    );

                    // Combine depen vectors
                    if (overlapped) {
                        collided = true;

                        // Valid correction
                        if (AddDepenDirection(in direction)) { // 
                            correction += direction * distance;
                        }
                        // Invalid correction, decide whether to disregard collision or stay in place
                        else { 
                            count = Physics.OverlapCapsuleNonAlloc(start, end, radius - tolerance, neighbours);
                            if (count > 0) {
                                if (neighbours[0] == thisCollider && count == 1) {
                                    movement = Vector3.zero; // Stay in place
                                    return;
                                } else {
                                    collided = false;
                                    hardCollided = false;
                                    return; // Move disregarding collision
                                }
                            } else {
                                movement = Vector3.zero; // Stay in place
                                return;
                            }
                        }
                    }
                } else {
                    hardCollided = Vector3.Angle(movement, correction) > hardCollisionAngle;
                    movement += correction; // No more colliding objects, corrected movement applied
                    return;
                }
            } else {
                hardCollided = Vector3.Angle(movement, correction) > hardCollisionAngle;
                movement += correction; // No more colliding objects, corrected movement applied
                return;
            }

        }
        Debug.Log("Trajectory collision failed");
        count = Physics.OverlapCapsuleNonAlloc(start, end, radius - tolerance, neighbours);
        if(count > 0) {
            if(neighbours[0] == thisCollider && count == 1) {
                movement = Vector3.zero; // Stay in place
                return;
            }
            else {
                collided = false;
                hardCollided = false;
                return; // Move disregarding collision
            }
        }
        else {
            movement = Vector3.zero; // Stay in place
            return;
        }
    }

    public Quaternion Rotate(in Quaternion rotation) {
        collided = false;
        if (!thisCollider) {
            return rotation; // Nothing to do without a Collider attached
        }

        float angle = Quaternion.Angle(Quaternion.identity, rotation);
        float tests = 180 / maxStepAngle;
        for(int i = 1; i < 180 / maxStepAngle; i++) {
            if (!(angle > i * maxStepAngle)) {
                tests = i;
                break;
            }
        }

        currentRotation = thisCollider.transform.localRotation;
        for(float i = 0; i < tests; i++) {
            probedRotation = Quaternion.Lerp(thisCollider.transform.localRotation, rotation, (i + 1f)/tests);
            if(!CheckRotation(probedRotation)) {
 
                collided = true;
                return currentRotation;
            }
            currentRotation = probedRotation;
        }
        return rotation;
    }
    
    public bool CheckRotation(in Quaternion rotation) {
        if (!thisCollider)
            return true; // Nothing to do without a Collider attached

        float radius = thisCollider.radius;
        float height = thisCollider.height - (2 * radius);

        start = rotation * (thisCollider.center + (Vector3.down * height / 2)) + thisCollider.transform.position;
        end = rotation * (thisCollider.center + (Vector3.up * height / 2)) + thisCollider.transform.position;

        sphere1.transform.position = start;
        sphere2.transform.position = end;
        int count = Physics.OverlapCapsuleNonAlloc(start, end, radius - tolerance, neighbours);
        if (count > 0) {
            var collider = neighbours[0];
            for (int i = 1; i < count; ++i) {
                if (collider == thisCollider) {
                    collider = neighbours[i];
                } else {
                    break;
                }
            }
            if (collider != thisCollider) {
                return false;                    
            } else {
                return true; // No colliding objects, rotation valid
            }
        } else {
            return true; // No colliding objects, rotation valid
        }
    }

    private bool AddDepenDirection(in Vector3 vec) {
        for (int i = 0; i < dePenVectorCount; i++) {
            if (Vector3.Dot(dePenetrationVectors[i], vec) < 0) {
                dePenetrationVectors[dePenVectorCount] = vec;
                dePenVectorCount++;
                return false;
            }
        }
        dePenetrationVectors[dePenVectorCount] = vec;
        dePenVectorCount++;
        return true;
    }

    //Check movement and corrects the movement only along the direction of the movement
    public void MoveRB(in Vector3 movement, out Vector3 correction) {

        collided = false;
        if (!thisCollider) {
            correction = movement;
            return; // Nothing to do without a Collider attached
        }


        float radius = thisCollider.radius;
        float height = thisCollider.height - (2 * radius);
        start = thisCollider.transform.rotation * (thisCollider.center + (Vector3.down * height / 2)) + thisCollider.transform.position;
        end = thisCollider.transform.rotation * (thisCollider.center + (Vector3.up * height / 2)) + thisCollider.transform.position;


        probedMovement = movement;

        float upperbound = 1.0f;
        float lowerbound = 0.0f;
        float current = 1.0f;
        int count;
        for (int i = 0; i < maxCollisionCount && lowerbound != upperbound; i++) {
            probedMovement = movement * current;
            count = Physics.OverlapCapsuleNonAlloc(start + probedMovement, end + probedMovement, radius - tolerance, neighbours);
            if (count == 0 || (count == 1 && neighbours[0] == thisCollider)) {
                lowerbound = current;
            } else {
                collided = true;
                upperbound = current;
            }
            current = ((upperbound - lowerbound) / 2f) + lowerbound;
        }
        correction = lowerbound * movement;
    }
}
