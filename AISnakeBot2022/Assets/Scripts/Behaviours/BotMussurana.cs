using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "AIBehaviours/BotMussuna")]
public class BotMussurana : AIBehaviour
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
        _moveDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        ownerMovement.StartCoroutine(UpdateDirEveryXSeconds(0));
    }

    //seria interessante ter um controlador com o colisor que define o mundo pra poder gerar pontos dentro desse colisor
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
                    _opponentsList.Add(collider.gameObject);
                }
            }
            else if(collider.tag == "Orb")
            {
                _orbsList.Add(collider.gameObject);
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
            _currentState = State.WANDER;
            ownerMovement.StartCoroutine(UpdateDirEveryXSeconds(6));
        }
    }

    private IEnumerator UpdateDirEveryXSeconds(float x)
    {
        yield return new WaitForSeconds(x);

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

        if (_currentState == State.WANDER)
        {
            ownerMovement.StartCoroutine(UpdateDirEveryXSeconds(x));
        }
    }
}
