using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "AIBehaviours/BotPapaCobra")]
public class BotPapaCobra : AIBehaviour
{
    private enum State
    {
        EATING, RUNNING, WANDER
    }

    private Collider2D[] _collidersList;

    private List<GameObject> _opponentsList = new List<GameObject>();
    private List<GameObject> _orbsList = new List<GameObject>();
    private State _currentState = State.EATING;
    private Transform _targetObject;
    private Transform _enemyObject;
    private Vector3 _moveDirection;

    public override void Init(GameObject own, SnakeMovement ownMove)
    {
        base.Init(own, ownMove);
        owner.gameObject.transform.parent.gameObject.name = "PAPA COBRA";
        ownerMovement.head.gameObject.GetComponent<SpriteRenderer>().color = Color.black;
        ownerMovement.head.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        _moveDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        ownerMovement.StartCoroutine(UpdateDirEveryXSeconds(0));
    }

    public override void Execute()
    {
        GetNearbyObjects();
        BotController();
    }

    private void GetNearbyObjects()
    {
        _collidersList = Physics2D.OverlapCircleAll(owner.transform.position, 10);

        _opponentsList.Clear();
        _orbsList.Clear();

        foreach (Collider2D collider in _collidersList)
        {
            if(collider.tag == "Body")
            {
                if(owner.gameObject.transform.parent != collider.gameObject.transform.parent)
                {
                    if(_opponentsList.Count > 0)
                    {
                        AddOpponent(collider.gameObject);
                    }
                    else
                    {
                        _opponentsList.Add(collider.gameObject);
                    }
                }
            }
            else if(collider.tag == "Orb")
            {
                _orbsList.Add(collider.gameObject);
            }
        }
    }

    private void AddOpponent(GameObject collider)
    {
        foreach (GameObject obj in _opponentsList)
        {
            if (obj.transform.parent == collider.transform.parent)
            {
                float oldDistance = Vector3.Distance(obj.transform.position, owner.transform.position);
                float newDistance = Vector3.Distance(collider.transform.position, owner.transform.position);

                if (newDistance < oldDistance)
                {
                    _opponentsList.Remove(obj);
                    _opponentsList.Add(collider.gameObject);
                }

                break;
            }
        }
    }

    private void BotController()
    {
        Move();

        if(IsInDanger() && _currentState != State.RUNNING)
        {
            ChangeStateAsync(0, State.RUNNING);
        }

        if(_currentState == State.WANDER)
        {
            WanderState();
        }
        else if(_currentState == State.EATING)
        {
            EatingState();
        }
        else if(_currentState == State.RUNNING)
        {
            RunningState();
        }
    }

    private async Task ChangeStateAsync(int delay, State newState)
    {
        await Task.Delay(delay);
        _currentState = newState;
    }

    private void Move()
    {
        owner.transform.position = Vector2.MoveTowards(owner.transform.position, _moveDirection, ownerMovement.speed * Time.deltaTime);
    }

    private void WanderState()
    {
        if(_orbsList.Count > 0)
        {
            GetOrb();
        }
    }

    private void EatingState()
    {
        VerifyTarget();
    }

    private void RunningState()
    {
        if(_enemyObject != null)
        {
            Vector3 vector = _enemyObject.gameObject.transform.position - owner.transform.position;
            _moveDirection = -vector;
            Debug.DrawRay(owner.transform.position , -vector, Color.blue, 0.1f);
        }

        if(IsCritical())
        {
            Dash();
        }

        if(!IsInDanger())
        {
            ChangeStateAsync(2000, State.WANDER);
            ownerMovement.StartCoroutine(UpdateDirEveryXSeconds(6));
        }
    }

    private bool IsInDanger()
    {
        foreach(GameObject opponent in _opponentsList)
        {
            if(Vector3.Distance(opponent.transform.position, owner.transform.position) < 6)
            {
                _enemyObject = opponent.gameObject.transform;
                return true;
            }
        }

        return false;
    }

    private bool IsCritical()
    {
        foreach(GameObject opponent in _opponentsList)
        {
            if(Vector3.Distance(opponent.transform.position, owner.transform.position) < 2)
            {
                return true;
            }
        }

        return false;
    }

    private void Dash()
    {
        if (ownerMovement.bodyParts.Count > 2)
        {
            Destroy(ownerMovement.bodyParts[ownerMovement.bodyParts.Count - 1].gameObject);
            Destroy(ownerMovement.bodyParts[ownerMovement.bodyParts.Count - 1]);
            ownerMovement.bodyParts.RemoveAt(ownerMovement.bodyParts.Count - 1);
            int nParts = ownerMovement.head.GetComponent<SnakeMovement>().bodyParts.Count;
            ownerMovement.gameObject.GetComponent<SpriteRenderer>().sortingOrder = nParts;
            ownerMovement.Eyes.GetComponent<SpriteRenderer>().sortingOrder = nParts+1;

            ownerMovement.isRunning = true;
            ownerMovement.speed = 10;
        }
    }

    private void GetOrb()
    {
        GameObject nearestOrb = null;

        foreach(GameObject orb in _orbsList)
        {
            if(nearestOrb == null)
            {
                nearestOrb = orb;
            }
            else
            {
                float lastDist = Vector3.Distance(nearestOrb.gameObject.transform.position, owner.gameObject.transform.position);
                float newDist = Vector3.Distance(orb.gameObject.transform.position, owner.gameObject.transform.position);

                if(newDist < lastDist)
                {
                    nearestOrb = orb;
                }
            }
        }

        direction = nearestOrb.gameObject.transform.position - owner.transform.position;
        direction.z = 0.0f;
        _targetObject = nearestOrb.transform;
        _moveDirection = _targetObject.transform.position;
        ChangeStateAsync(0, State.EATING);
    }

    private void VerifyTarget()
    {
        if(_targetObject == null)
        {
            ChangeStateAsync(0, State.WANDER);
            ownerMovement.StartCoroutine(UpdateDirEveryXSeconds(0));
            ChangeBodyColor();
        }
    }

    private void ChangeBodyColor()
    {
        for (int i = 0; i < ownerMovement.bodyParts.Count; i++)
        {
            ownerMovement.head.GetComponent<SnakeMovement>().bodyParts[i].gameObject.GetComponent<SpriteRenderer>().color = Color.black;
        }
    }

    private IEnumerator UpdateDirEveryXSeconds(float x)
    {
        yield return new WaitForSeconds(x);

        Debug.Log("NEW DIR");

        if (_currentState == State.WANDER)
        {
            ownerMovement.StopCoroutine(UpdateDirEveryXSeconds(x));
            randomPoint = new Vector3(
                    Random.Range(
                        Random.Range(-45, 45),
                        Random.Range(-45, 45)
                    ),
                    Random.Range(
                        Random.Range(-45, 45),
                        Random.Range(-45, 45)
                    ),
                    0
                );
            direction = randomPoint - owner.transform.position;
            direction.z = 0.0f;

            _moveDirection = direction;

            ownerMovement.StartCoroutine(UpdateDirEveryXSeconds(6));
        }
    }
}
