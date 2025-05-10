using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed;
    public Rigidbody2D rb;

    private Vector2 moveDirection;


    private MapManager mapManager;
    private Animator animator;


    public float detectionRange = 2f;


    private bool canOpenChest = false;
    public GameObject weaponMenu;
    public GameObject[] weapons;
    public GameObject currentWeapon;
    public GameObject weaponSlot1, weaponSlot2, weaponSlot3;
    public WeaponManager weaponManager;

    private GameObject[] chosenWeapons = new GameObject[3];

    public GameObject weaponSlot;
    public Text weaponSlotText;
    public Image weaponSlotImage;

    [SyncVar]
    public int playerID = -1;

    [SyncVar]
    public int opponentID = -1;


    void Start()
    {
        if (isLocalPlayer)
        {
            weaponManager = FindObjectOfType<WeaponManager>();
            mapManager = FindObjectOfType<MapManager>();
            animator = GetComponent<Animator>();

            weaponMenu = weaponManager.weaponMenu;
            weaponSlot1 = weaponManager.weaponSlot1;
            weaponSlot2 = weaponManager.weaponSlot2;
            weaponSlot3 = weaponManager.weaponSlot3;

            weaponSlot = weaponManager.weaponSlot;
            weaponSlotText = weaponManager.weaponSlotText;
            weaponSlotImage = weaponManager.weaponSlotImage;

            mapManager.openButton.GetComponent<Button>().onClick.AddListener(OnClickOpenChest);
            mapManager.attackButton.GetComponent<Button>().onClick.AddListener(OnClickAttack);
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        ProcessInputs();
        UpdateAnimations();
        CheckForNearbyChests();
        CheckForNearbyPlayers();
        UpdateWeaponSlotUI();


        if (playerID == 1)
        {
            Debug.Log("I am Player 1, enemy is Player " + opponentID);
        }
        else if (playerID == 2)
        {
            Debug.Log("I am Player 2, enemy is Player " + opponentID);
        }

    }

    void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            Move();
        }
    }

    void ProcessInputs()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector2(moveX, moveY).normalized;
    }

    void Move()
    {
        rb.linearVelocity = moveDirection * moveSpeed;
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        bool isWalking = moveDirection.magnitude > 0.01f;
        animator.SetBool("isWalking", isWalking);

        if (isWalking)
        {
            animator.SetFloat("moveX", moveDirection.x);
            animator.SetFloat("moveY", moveDirection.y);
        }
        if (moveDirection.x > 0.01f)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f); // face right
        }
        else if (moveDirection.x < -0.01f)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f); // face left
        }

    }

    void CheckForNearbyChests()
    {
        Vector3 playerWorldPosition = transform.position;
        Vector3Int cellPosition = mapManager.map.WorldToCell(playerWorldPosition);

        TileBase tile = mapManager.map.GetTile(cellPosition);

        if (tile != null && mapManager.dataFromTiles.TryGetValue(tile, out TileData tileData))
        {
            canOpenChest = true;
            mapManager.openButton.SetActive(tileData.chest);
        }
        else
        {
            canOpenChest = false;
            mapManager.openButton.SetActive(false);
        }
    }

    void CheckForNearbyPlayers()
    {
        foreach (var playerObj in FindObjectsOfType<PlayerMovement>())
        {
            if (playerObj != this)
            {
                float distance = Vector2.Distance(transform.position, playerObj.transform.position);
                if (distance <= detectionRange && currentWeapon != null)
                {
                        mapManager.attackButton.SetActive(true);
                        Debug.Log("Player nearby!");
                        return;
                }
                else
                {
                    mapManager.attackButton.SetActive(false);
                }
            }
        }
    }

    public void OnClickOpenChest()
    {
        if (canOpenChest && isLocalPlayer)
        {
            weaponMenu.SetActive(true);

            // Pick 3 random weapons
            List<GameObject> weaponPool = new List<GameObject>(weapons);
            for (int i = 0; i < 3; i++)
            {
                int index = Random.Range(0, weaponPool.Count);
                GameObject weapon = weaponPool[index];
                weaponPool.RemoveAt(index); // Ensure unique picks
                chosenWeapons[i] = weapon;

                // Get UI slot and populate it
                GameObject slot = i == 0 ? weaponSlot1 : i == 1 ? weaponSlot2 : weaponSlot3;

                Text nameText = slot.transform.Find("Name").GetComponent<Text>();
                Image image = slot.transform.Find("Image").GetComponent<Image>();
                WeaponObject weaponObj = weapon.GetComponent<WeaponObject>();

                nameText.text = weaponObj.weaponName;
                image.sprite = weaponObj.weaponSprite;

                int captureIndex = i; // To avoid closure issue
                slot.GetComponent<Button>().onClick.RemoveAllListeners();
                slot.GetComponent<Button>().onClick.AddListener(() => OnSelectWeapon(captureIndex));
            }
        }
    }

    public void OnSelectWeapon(int index)
    {
        GameObject selectedWeapon = chosenWeapons[index];
        WeaponObject weaponData = selectedWeapon.GetComponent<WeaponObject>();
        currentWeapon = selectedWeapon;
        Debug.Log("You selected: " + weaponData.weaponName);

        // Optionally hide the menu after selection
        weaponMenu.SetActive(false);

        // TODO: trigger your weapon equip logic here
    }

    void UpdateWeaponSlotUI()
    {
        if (currentWeapon != null)
        {
            weaponSlot.SetActive(true);
            WeaponObject weaponObj = currentWeapon.GetComponent<WeaponObject>();
            weaponSlotText.text = weaponObj.weaponName;
            weaponSlotImage.sprite = weaponObj.weaponSprite;
        }
        else
        {
            weaponSlot.SetActive(false);
        }
    }

    public void OnClickAttack()
    {
        CmdRequestDamagePlayer(opponentID);
    }

    [Command]
    public void CmdRequestDamagePlayer(int opponentID)
    {
        FindObjectOfType<GameManager>().DamagePlayer(opponentID);
    }

}
