/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PuzzleBox
{
    [RequireComponent(typeof(PolygonCollider2D))]
    public class CustomTileCollision : MonoBehaviour
    {
        PolygonCollider2D coll;

        // Start is called before the first frame update
        void Start()
        {
            coll = GetComponent<PolygonCollider2D>();

            Tilemap tilemap = GetComponentInParent<Tilemap>();

            if (tilemap != null)
            {
                Vector3Int cell = tilemap.WorldToCell(transform.position);
                TileBase tile = tilemap.GetTile(cell);
                if (tile != null)
                {
                    TileData td = new TileData();
                    tile.GetTileData(cell, tilemap, ref td);

                    if (td.sprite != null)
                    {
                        for (int i = 0; i < td.sprite.GetPhysicsShapeCount(); i++)
                        {
                            var path = new List<Vector2>();
                            td.sprite.GetPhysicsShape(i, path);
                            coll.SetPath(i, path);
                        }
                    }
                }
            }
        }
    }
}

