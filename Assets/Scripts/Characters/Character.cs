﻿using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using GameConfig;
using GameUtils;
using UnityEngine.UI;

namespace GameCharacters
{
    public class Character : MonoBehaviour
    {
        protected Animator animator;
        protected Rigidbody playerRigidbody;

        // Character components
        protected CharacterAttrs attrs;
        protected CharacterHP characterHP;
        protected CharacterSkills skills;
        protected CharacterState state;
        protected CharacterAnimation anim;
        protected CharacterExtendObj extendObj;
        protected Transform[] weaponCenter;

        public string characterName;

        public void Start()
        {
            animator = GetComponent<Animator>();
            playerRigidbody = GetComponent<Rigidbody>();
            extendObj = GetComponent<CharacterExtendObj>();

            // Init character components
            Assembly assembly = Assembly.GetExecutingAssembly();
            string className = "GameCharacters." + characterName + "Attrs";
            attrs = (CharacterAttrs)assembly.CreateInstance(className);

            className = "GameCharacters." + characterName + "Skills";
            skills = (CharacterSkills)assembly.CreateInstance(className);

            // set HP. need a check
            Slider HPSlider = transform.Find("Canvas/HealthSlider").GetComponent<Slider>();
            Image fillImage = transform.Find("Canvas/HealthSlider/Fill Area/Fill").GetComponent<Image>();
            characterHP = new CharacterHP(attrs.maxHP, HPSlider, fillImage);
            characterHP.OnEnable();

            state = new CharacterState();
            anim = new CharacterAnimation();

            // get weapon Collider
            GetWeaponsCenter();
        }

        protected virtual void GetWeaponsCenter() { }

        public virtual void CheckCoolTime()
        {
            if (skills.dashAttrs.waitTime < skills.dashAttrs.coolTime
                && skills.dashAttrs.waitTime >= 0)
                skills.dashAttrs.waitTime += Time.deltaTime;
            else if (skills.dashAttrs.waitTime >= skills.dashAttrs.coolTime)
                skills.dashAttrs.waitTime = -1;
        }

        public void Move(float h, float v)
        {
            if ((h != 0 || v != 0) && attrs.walkSpeed > 0f)
            {
                state.IsRun = true;
                ChangeAnim(anim.run, true);
                Vector3 movement = new Vector3(h, 0f, v);
                movement = movement.normalized * attrs.walkSpeed * Time.deltaTime;
                playerRigidbody.MovePosition(transform.position + movement);
                Turn(h, v);
                return;
            }

            state.IsRun = false;
            ChangeAnim(anim.run, false);
        }

        public void Turn(float h, float v)
        {
            Vector3 rotation = new Vector3(h, 0f, v);
            if (rotation.magnitude > 0.1f)
                playerRigidbody.MoveRotation(Quaternion.LookRotation(-rotation));
        }

        public virtual void LightAttack()
        {
            if (!state.IsGround || state.IsDash || state.IsLightAttack)
                return;
            CancelInvoke();
        }

        public void Dash()
        {
            if (!state.IsGround || !state.IsRun || state.IsAttack)
                return;

            if (skills.dashAttrs.waitTime >= 0)
                return;

            state.IsDash = true;
            //dash here
            Vector3 movement = -transform.TransformDirection(Vector3.forward).normalized;
            movement = movement * skills.dashAttrs.distance;
            Hashtable args = new Hashtable();
            args.Add("speed", skills.dashAttrs.speed);
            args.Add("easeType", skills.dashAttrs.moveWay);
            args.Add("onstart", "ChangeDashAnim");
            args.Add("onstartparams", true);
            args.Add("oncomplete", "ChangeDashAnim");
            args.Add("oncompleteparams", false);

            //args.Add("onstarttarget", gameObject);
            //args.Add("onupdatetarget", gameObject);
            //args.Add("oncompletetarget", gameObject);
            args.Add("position", transform.position + movement);
            iTween.MoveTo(gameObject, args);
            skills.dashAttrs.waitTime = 0;
        }

        public void TakeDamage(float damage)
        {
            if (characterHP.Change(-damage) == 0)
            {
                // player daed
            }
            else
            {
                // player hurt
            }
        }

        public void Death()
        {

        }

        public float ChangeHP(float amount = 0)
        {
            return characterHP.Change(amount);
        }

        public void ChangeAnim(string paramName, bool value)
        {
            animator.SetBool(paramName, value);
        }

        public void ChangeAnim(string paramName, int value)
        {
            animator.SetInteger(paramName, value);
        }

        public void ChangeAnim(string paramName, float value)
        {
            animator.SetFloat(paramName, value);
        }
        public void ChangeAnim(string paramName)
        {
            animator.SetTrigger(paramName);
        }
        public void ChangeDashAnim(bool v)
        {
            animator.SetBool("dash", v);
        }
        public void AnimEnd()
        {
            int nameHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;

            if (nameHash == anim.lightAttackAnim)
                state.IsLightAttack = false;
            //else if (nameHash == anim.dashAnim)
            //{
            //    state.IsDash = false;
            //}

        }
    }

}
