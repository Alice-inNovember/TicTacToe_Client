﻿using System;
using System.Collections.Generic;
using System.Linq;
using Network;
using UI;
using UnityEngine;
using Util.EventSystem;
using EventType = Util.EventSystem.EventType;

namespace Game
{
	public class TicTacToe : MonoBehaviour, IEventListener
	{
		[SerializeField] private List<TileBlock> tileBlocks;

		public void OnEvent(EventType eventType, Component sender, object param = null)
		{
			Debug.Log(eventType);
			switch (eventType)
			{
				case EventType.ProgramStart:
					break;
				case EventType.ServerConnection:
					break;
				case EventType.GameStart:
					break;
				case EventType.PlayerTileClicked:
					if (param != null)
						OnTileClick((Vector2Int)param, GameManager.Instance.playerTileType);
					break;
				case EventType.EnemyTileClicked:
					if (param != null)
						OnTileClick((Vector2Int)param, GameManager.Instance.enemyTileType);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
			}
		}
		
		private void Start()
		{
			EventManager.Instance.AddListener(EventType.GameStart, this);
			EventManager.Instance.AddListener(EventType.PlayerTileClicked, this);
			EventManager.Instance.AddListener(EventType.EnemyTileClicked, this);
		}

		private void OnTileClick(Vector2Int id, TileType hwo)
		{
			if (GameManager.Instance.turn != hwo)
			{
				Debug.Log("Invalid Turn");
				return;
			}
			if (hwo == GameManager.Instance.playerTileType)
				NetworkManager.Instance.Send(new Message(EMessageType.MT_USER_ACTION, new string($"{id.x},{id.y}")));
			TileSet(id);
			TurnSwap();
			CheckComplete();
		}

		private void SetAllBlockInteractable(bool interactable = false)
		{
			foreach (var block in tileBlocks)
				block.SetBlock(block.Type, interactable);
		}

		private void TileSet(Vector2Int id)
		{
			tileBlocks[id.x].SetTile(id.y, GameManager.Instance.turn, false);
			tileBlocks[id.x].CheckComplete(); //작은 TTT 의 결과 확인
			// CheckComplete(); // 본인의 결과 확인
			var nextBlockID = id.y;
			if (tileBlocks[nextBlockID].IsComplete())
			{
				foreach (var block in tileBlocks)
					block.SetBlock(block.Type, !block.IsComplete());
			}
			else
			{
				SetAllBlockInteractable();
				tileBlocks[nextBlockID].SetBlock(tileBlocks[nextBlockID].Type, true);
			}
		}

		private void TurnSwap()
		{
			if (GameManager.Instance.turn == TileType.O)
				GameManager.Instance.turn = TileType.X;
			else
				GameManager.Instance.turn = TileType.O;
			UIManager.Instance.SetCurrentTurnText();
		}

		public void CheckComplete()
		{
			var tileType = TileType.Null;
			for (var i = 0; i < 3; i++)
			{
				var h = 3 * i;
				var v = i;
				Debug.Log($"H {h} V {v}");
				
				//가로 세로 체크
				if (tileBlocks[h].Type == tileBlocks[h + 1].Type && tileBlocks[h].Type == tileBlocks[h + 2].Type)
					tileType = tileBlocks[h].Type;
				else if (tileBlocks[v].Type == tileBlocks[v + 3].Type && tileBlocks[v].Type == tileBlocks[v + 6].Type)
					tileType = tileBlocks[v].Type;

				if (tileType != TileType.Null)
					break;
			}
			if (tileType == TileType.Null)
			{
				//대각선
				if (tileBlocks[0].Type == tileBlocks[4].Type && tileBlocks[0].Type == tileBlocks[8].Type)
					tileType = tileBlocks[0].Type;
				else if (tileBlocks[2].Type == tileBlocks[4].Type && tileBlocks[2].Type == tileBlocks[6].Type)
					tileType = tileBlocks[2].Type;
			}
			//무승부
			if (tileType == TileType.Null && tileBlocks.Count(block => block.Type == TileType.Null) == 0)
				GameManager.Instance.GameOver(TileType.Null);
			//결과
			if (tileType == TileType.Null)
				return;
			NetworkManager.Instance.Send(new Message(EMessageType.MT_GAME_RESULT, "Game End"));
			GameManager.Instance.GameOver(tileType);
		}
	}
}