﻿using System.Collections;
using UnityEngine;

namespace Game.Scripts.Player
{
    public class PlayerMovementController : MonoBehaviour
    {
        private Rigidbody2D playerRb;
        private PlayerInputManager inputManager;
        private PlayerColliderManager colliderManager;
        private new BoxCollider2D collider;
        private GameManager.GameManager gameManager;
        private ScoreManager scoreManager;

        // Collider Normal offset = 0 e 0.95 size = 0.5 e 1.9

        private Vector2 targetVelocity;

        public int PlayerDirection { private set; get; } = 1;

        public float inputAverageTime = 0f;
        private float maxSpeed = 10f;
        private float movementTimer = -1f;

        public bool isFalling;

        [Header("Movimentação")]
        [Range(0f, 1f)]
        [SerializeField] private float accelerationRate;
        [Range(1f, 5f)]
        [SerializeField] private float decelerationForce = 2f;
        [SerializeField] private float fallSpeedDeceleration;
        [Range(0.1f, 1f)]
        [Tooltip("Seta o time out entre os inputs necessário para o personagem começar a frear")]
        [SerializeField] float movementTimeOut = 1f;
        [SerializeField] float maxSpeedTimeOut;
        [SerializeField] float velocidadeSuperLenta = 0.5f;
        [SerializeField] float velocidadeLenta = 0.35f;
        [SerializeField] float velocidadeMédia = 0.22f;
        [SerializeField] float velocidadeAlta = 0.12f;
        [SerializeField] float velocidadeSuperAlta = 0.03f;
        [Range(5f, 15f)]
        [SerializeField] private float slopeSpeed;


        [Header("Pulo")]
        [SerializeField] bool canJump = true;
        [SerializeField] bool isJumping = false;
        [Range(5f, 20f)]
        [SerializeField] float jumpForce = 5f;
        [Range(5f, 20f)]
        [SerializeField] float gravityForce = 5f;
        [SerializeField] float jumpAbortDecceleration = 4f;

        bool isCrouched;
        Vector2 normalColliderOffset;
        Vector2 normalColliderSize;
        [Header("Agachar")]
        [Range(0f, 1f)]
        [SerializeField] float crouchResizePercentage = 0.5f;
        [Range(1f, 10f)]
        [SerializeField] float crouchMovementSpeed;
        
        [Header("Estabilidade do Jogador")]
        public float balanceAmount = 1.0f;
        [Range(0f, 1f)]
        [SerializeField]private float balanceRechargeRate;
        [Range(0f, 1f)]
        [SerializeField]private float unbalancePercentageRate;

        public float SlopeSpeed => slopeSpeed;

        public float UnbalancePercentageRate => unbalancePercentageRate;

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------

        // Start is called before the first frame update
        public void Start()
        {
            playerRb = GetComponent<Rigidbody2D>();
            inputManager = GetComponent<PlayerInputManager>();
            colliderManager = GetComponentInChildren<PlayerColliderManager>();
            collider = GetComponent<BoxCollider2D>();
            gameManager = FindObjectOfType<GameManager.GameManager>();
            scoreManager = FindObjectOfType<ScoreManager>();

            normalColliderOffset = collider.offset;
            normalColliderSize = collider.size;
        
            canJump = true;
        }

        public void Update()
        {
            if (balanceAmount < 1)
            {
                balanceAmount += balanceRechargeRate * Time.deltaTime;
            }
            else
            {
                balanceAmount = 1;
            }
            // Flip
            if (inputManager.IsSwipeDirectionButtonDown())
            {
                FlipDirection();
            }

            if (!gameManager.IsGameRunning)
            {
                return;
            }

            //Cair
            if (inputManager.fall)
            {
                Debug.Log("Cai");
                scoreManager.fallNumber += 1;
                StartCoroutine(Fall());
                inputManager.fall = false;
            }

            // Pulo
            canJump = colliderManager.isGrounded && !colliderManager.isMovingUp && !colliderManager.isMovingDown;
            isJumping = !colliderManager.isGrounded;

            if (inputManager.IsJumpButtonDown() && canJump)
            {
                Jump();
            }
            if(inputManager.IsJumpButtonReleased())
            {
                AbortJump();
            }

            //Agachar
            if (inputManager.IsCrouchButtonDown())
            {
                Debug.Log("Agachei");
                Crouch();
            }
            if (inputManager.IsCrouchButtonReleased())
            {
                Debug.Log("Desagachei");
                Uncrouch();
            }

            // Andar
            // Calculando a média entre os dois cliques
            inputAverageTime = Mathf.Abs(inputManager.aTime - inputManager.dTime);

            // Normaliza o valor da media do input e seta a velocidade máxima para cada media
            NormalizeInputAverageTime();

            // Reseta os valores de input
            if (inputManager.aWasPressed && inputManager.dWasPressed)
            {
                movementTimer = Time.time;
                inputManager.aWasPressed = false;
                inputManager.dWasPressed = false;
            }
        }

        public void FixedUpdate()
        {
            if (!gameManager.IsGameRunning)
            {
                return;
            }
            
            if (isJumping || colliderManager.isMovingDown)
            {
                ApplyGravity();
            }

            if (isFalling)
            {
                return;
            }

            if (colliderManager.isMovingDown || colliderManager.isMovingUp)
                return;
            
            // Move e desacelera o personagem conforme a média de tempo dos inputs
            if ((Time.time - movementTimer <= movementTimeOut) && !isFalling)
            {
                // Acelerando
                if (isCrouched) { targetVelocity = new Vector2(crouchMovementSpeed * PlayerDirection, playerRb.velocity.y); }
                else { targetVelocity = new Vector2(maxSpeed * PlayerDirection, playerRb.velocity.y); }

                if (targetVelocity.x > maxSpeed)
                {
                    return;
                }

                playerRb.velocity = Vector2.Lerp(playerRb.velocity, targetVelocity, Time.deltaTime * accelerationRate);
            }
            else
            {
                if (Mathf.Approximately(playerRb.velocity.x, 0) || playerRb.velocity.x < 0*PlayerDirection)
                {
                    playerRb.velocity = new Vector2(0, playerRb.velocity.y);
                    inputManager.aTime = 0f;
                    inputManager.dTime = 0f;
                }
                else if(!isJumping)
                {
                    // Desacelerando
                    ApplyDeceleration();
                }
            }
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------------
    
        private void NormalizeInputAverageTime()
        {
            if (inputAverageTime > velocidadeSuperLenta) // Super lento
            {
                maxSpeed = 2f;
                inputAverageTime = velocidadeSuperLenta;
            }
            else if (inputAverageTime > velocidadeLenta) // lento
            {
                maxSpeed = 4f;
                inputAverageTime = velocidadeLenta;
            }
            else if (inputAverageTime > velocidadeMédia) // Médio
            {
                maxSpeed = 7f;
                inputAverageTime = velocidadeMédia;
            }
            else if (inputAverageTime > velocidadeAlta) // Rápido
            {
                maxSpeed = 10f;
                inputAverageTime = velocidadeAlta;
            }
            else if (inputAverageTime > velocidadeSuperAlta) // Super Rápido
            {
                maxSpeed = 15f;
                inputAverageTime = velocidadeSuperAlta;
            }
            else if(inputAverageTime > 0f)
            {
                StartCoroutine(Fall());
            }
        }

        private void FlipDirection()
        {
            playerRb.transform.localScale = new Vector3(-playerRb.transform.localScale.x, playerRb.transform.localScale.y, playerRb.transform.localScale.z);
            PlayerDirection = PlayerDirection == 1 ? -1 : 1;
            inputManager.aTime = 0f;
            inputManager.dTime = 0f;
            inputManager.aWasPressed = false;
            inputManager.dWasPressed = false;
        }

        private void Jump()
        {
            if (!isCrouched)
            {
                playerRb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                inputManager.aWasPressed = false;
                inputManager.dWasPressed = false;
            }
        }

        public void AbortJump()
        {
            if (isJumping)
            {
                playerRb.velocity = new Vector2(playerRb.velocity.x, Mathf.Lerp(playerRb.velocity.y, 0, jumpAbortDecceleration*Time.deltaTime));
            }
        }

        private void ApplyGravity()
        {
            playerRb.AddForce(Vector2.up * -gravityForce);
        }

        public void ApplyDeceleration()
        {
            targetVelocity = new Vector2(decelerationForce * maxSpeed * -PlayerDirection, playerRb.velocity.y);
            playerRb.velocity = Vector2.Lerp(playerRb.velocity, targetVelocity, Time.deltaTime * accelerationRate);
        }

        private void Crouch()
        {
            if (!isJumping)
            {
                collider.size = new Vector2(collider.size.x, collider.size.y * crouchResizePercentage);
                collider.offset = new Vector2(collider.offset.x, collider.offset.y - 0.45f);
                isCrouched = true;
                canJump = false;
                inputManager.aWasPressed = false;
                inputManager.dWasPressed = false;
            }
        }

        private void Uncrouch()
        {
            collider.offset = normalColliderOffset;
            collider.size = normalColliderSize;
            isCrouched = false;
            canJump = true;
            inputManager.aWasPressed = false;
            inputManager.dWasPressed = false;
        }

        private IEnumerator Fall()
        {
            isFalling = true;
            while(playerRb.velocity != Vector2.zero && playerRb.velocity.x > 0*PlayerDirection)
            {
                targetVelocity = new Vector2(fallSpeedDeceleration * maxSpeed * -PlayerDirection, playerRb.velocity.y);
                playerRb.velocity = Vector2.Lerp(playerRb.velocity, targetVelocity, Time.deltaTime * accelerationRate);
                yield return null;
            }
            isFalling = false;

            inputManager.aWasPressed = false;
            inputManager.dWasPressed = false;

            StopCoroutine(Fall());
        }
    }
}
