﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using DataAccessLayer.Models;
using ServiceLayer;
using System;
using System.Text.Json;
using Newtonsoft.Json;

namespace REST.Controllers
{
    [ApiController]
    public class ReceiptController : ControllerBase
    {
        private readonly ILogger<ReceiptController> _logger;
        private readonly APIHelper api;
        private readonly ReceiptService rs;
        private readonly HashSet<string> attributes;

        public ReceiptController(ILogger<ReceiptController> logger)
        {
            _logger = logger;
            api = new APIHelper();
            rs = new ReceiptService();

            attributes = new HashSet<string>
            {
                "TID", "ImgURL", "PurchaseDate", "Notes"
            };
        }

        // Retreive a receipt with the given RID
        // returns Http 204 NoContent if doesn't exist
        [Route("api/Receipt/RID={RID}&apikey={apikey}")]
        [HttpGet]
        public IActionResult GetByID(long RID, string apikey)
        {
            if (!api.validAPIKey(apikey))
            {
                return new UnauthorizedObjectResult("Invalid API key");
            }

            if(RID < 1)
            {
                return BadRequest();
            }

            return api.serveJson(rs.getJSON(rs.getUsingID(RID)));
        }


        // Delete a Receipt, no effect if a receipt with the given RID doesn't exist
        [Route("api/Receipt/RID={RID}&apikey={apiKey}")]
        [HttpDelete]
        public IActionResult DeleteByID(long RID, string apiKey)
        {
            if (!api.validAPIKey(apiKey))
            {
                return new UnauthorizedObjectResult("Invalid API key");
            }
            if (RID < 1)
            {
                return BadRequest();
            }
            rs.delete(RID);
            return new OkResult();
        }


        // Create a new Receipt with the given RID and request data.
        // Returns Http 409 Conflict if already exists
        [Route("api/Receipt/RID={RID}&apikey={apiKey}")]
        [HttpPost]
        public IActionResult CreateByID(long RID, string apiKey, [FromBody] JsonElement reqBody)
        {
            if (!api.validAPIKey(apiKey))
            {
                return new UnauthorizedObjectResult("Invalid API key");
            }
            if (RID < 1)
            {
                return BadRequest();
            }

            // Validate that the POST request contains all necessary attributes to create a NEW Receipt and nothing more
            Dictionary<string, object> req = JsonConvert.DeserializeObject<Dictionary<string, object>>(Convert.ToString(reqBody));
            HashSet<string> reqAttributes = new HashSet<string>(req.Keys);
            if (!reqAttributes.SetEquals(attributes))
            {
                return BadRequest();
            }

            // POST should be used only to create a new Receipt, not allowed if Receipt with given RID already exists
            Receipt r = rs.getUsingID(RID);
            if (r != null)
            {
                return Conflict();
            }

            // Create the Receipt with the given RID using the POST payload
            try
            {
                r = new Receipt(
                        RID: RID,
                        TID: Convert.ToInt64(req["TID"]),
                        ImgURL: Convert.ToString(req["ImgURL"]),
                        PurchaseDate: Convert.ToDateTime(req["PurchaseDate"]),
                        Notes: Convert.ToString(req["Notes"]),
                        newlyCreated: true
                    );
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return BadRequest();
            }


            // Write the new Receipt
            rs.write(r);
            return new OkResult();
        }

        // Create a new Receipt with the next available RID
        [Route("api/Receipt&apikey={apiKey}")]
        [HttpPost]
        public IActionResult Create(string apiKey, [FromBody] JsonElement reqBody)
        {
            long RID = rs.getNextAvailID();
            IActionResult res = CreateByID(RID, apiKey, reqBody);
            // Request was invalid, failed to create
            if (!(res is OkResult))
            {
                return res;
            }

            // Otherwise return the TID of the newly created transaction
            return api.serveJson(api.getSingleAttrJSON("RID", RID.ToString()));
        }
   

        // Update an existing Receipt or create if not exists
        [Route("api/Receipt/RID={RID}&apikey={apiKey}")]
        [HttpPut]
        public IActionResult UpdateAllByID(long RID, string apiKey, [FromBody] JsonElement reqBody)
        {
            if (!api.validAPIKey(apiKey))
            {
                return new UnauthorizedObjectResult("Invalid API key");
            }
            if (RID < 1)
            {
                return BadRequest();
            }

            Dictionary<string, object> req = JsonConvert.DeserializeObject<Dictionary<string, object>>(Convert.ToString(reqBody));
            HashSet<string> reqAttributes = new HashSet<string>(req.Keys);
            Receipt r = rs.getUsingID(RID);

            // Create the Receipt with the given RID if it doesn't exist
            if (r == null)
            {
                // PUT requires request to provide key,value pairs for EVERY Receipt attribute 
                if (!reqAttributes.SetEquals(attributes))
                {
                    return BadRequest();
                }
                try
                {
                    r = new Receipt(
                            RID: RID,
                            TID: Convert.ToInt64(req["TID"]),
                            ImgURL: Convert.ToString(req["ImgURL"]),
                            PurchaseDate: Convert.ToDateTime(req["PurchaseDate"]),
                            Notes: Convert.ToString(req["Notes"]),
                            newlyCreated: true
                        );
                }
                // Formatting or improper data typing raised exception, bad request
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return BadRequest();
                }

                // Write the new Receipt
                rs.write(r);
            }

            // Otheriwse fufill the PUT request and update the corresponding Receipt 
            else
            {
                // HTTP PUT request to update an EXISTING Receipt requires ALL fields of the Reciept to be specified
                if (!reqAttributes.SetEquals(attributes))
                {
                    return BadRequest();
                }

                // TID is never modifiable
                try
                {
                    r.setImageLink(Convert.ToString(req["ImgURL"]));
                    r.setPurchaseDate(Convert.ToDateTime(req["PurchaseDate"]));
                    r.setNote(Convert.ToString(req["Notes"]));
                }
                // Formatting or improper data typing raised exception, bad request
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return BadRequest();
                }

                // Write changes, if any
                rs.update(r);
            }

            return new OkResult();
        }


        // Modify an existing Receipt without specifying all attributes in payload,
        // returns Http 404 Not found if doesn't exist
        [Route("api/Receipt/RID={RID}&apikey={apiKey}")]
        [HttpPatch]
        public IActionResult UpdateByID(long RID, string apiKey, [FromBody] JsonElement reqBody)
        {
            if (!api.validAPIKey(apiKey))
            {
                return new UnauthorizedObjectResult("Invalid API key");
            }
            if (RID < 1)
            {
                return BadRequest();
            }

            // Validate the attributes of the PATCH request, each attribute specified
            // in the request must be an attribute of a Receipt
            Dictionary<string, object> req = JsonConvert.DeserializeObject<Dictionary<string, object>>(Convert.ToString(reqBody));
            HashSet<string> reqAttributes = new HashSet<string>(req.Keys);
            if (!api.validAttributes(attributes, reqAttributes))
            {
                return BadRequest();
            }

            Receipt r = rs.getUsingID(RID);

            // Http POST cannot update a Receipt that does not exist
            if (r == null)
            {
                return NotFound();
            }

            // Otherwise fufill the PATCH request and update the corresponding Receipt
            // Http PATCH may only specify a few attributes to update or provide all of them
            // TID is not an updatable attribute

            // Update the Receipt with the specified POST attributes
            try
            {
                if (reqAttributes.Contains("ImgURL"))
                {
                    r.setImageLink(Convert.ToString(req["ImgURL"]));
                }
                if (reqAttributes.Contains("PurchaseDate"))
                {
                    r.setPurchaseDate(Convert.ToDateTime(req["PurchaseDate"]));
                }
                if (reqAttributes.Contains("Notes"))
                {
                    r.setNote(Convert.ToString(req["Notes"]));
                }
            }
            // Formatting or improper data typing raised exception, bad request
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return BadRequest();
            }

            // Write changes, if any
            rs.update(r);
            return new OkResult();
        }

    }
}