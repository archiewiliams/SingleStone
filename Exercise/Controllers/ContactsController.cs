using AutoMapper;
using Exercise.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Exercise.Controllers
{
    /// <summary>
    /// This is the Contacts Controller - where the magic happens
    /// </summary>
    public class ContactsController : ApiController
    {

        /// <summary>
        /// List all contacts
        /// </summary>
        /// <returns></returns>
        // GET api/contacts
        [HttpGet]
        [Route("api/Contacts")]
        public IHttpActionResult Get()
        {
            using (var db = new ExerciseEntities())
            {
                try
                {
                    IEnumerable<Models.Contact> contacts = (from co in db.Contacts
                                                            join na in db.Names on co.ContactID equals na.ContactID
                                                            join ad in db.Addresses on co.ContactID equals ad.AddressID
                                                            join ph in db.Phones on ad.ContactID equals ph.ContactID
                                                            select new Models.Contact
                                                            {
                                                                id = co.ContactID,
                                                                name = new Models.Name
                                                                {
                                                                    first = na.First,
                                                                    middle = na.Middle,
                                                                    last = na.Last
                                                                },
                                                                address = new Models.Address
                                                                {
                                                                    street = ad.Street,
                                                                    city = ad.City,
                                                                    state = ad.State,
                                                                    zip = ad.Zip
                                                                },
                                                                phone = (from p in db.Phones
                                                                         where p.ContactID == co.ContactID
                                                                         select new Models.Phone
                                                                         {
                                                                             phoneid = p.PhoneID,
                                                                             number = p.Number,
                                                                             type = p.Type.ToString()
                                                                         }).ToList(),
                                                                email = co.Email
                                                            }).ToList();

                    contacts = contacts.GroupBy(x => x.id).Select(x => x.First());

                    return Ok(contacts);
                }
                catch (Exception ex)
                {
                    return InternalServerError();
                }
            }
        }

        
        /// <summary>
        /// Get a specific contact
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // GET api/contacts/5
        [HttpGet]
        [Route("api/Contacts/{id}")]
        public IHttpActionResult Get(int id)
        {
            try
            {
                if (id <= 0) return BadRequest();

                Models.Contact contact = new Models.Contact();
                using (var db = new ExerciseEntities())
                {
                    contact = (from co in db.Contacts
                               join na in db.Names on co.ContactID equals na.ContactID
                               join ad in db.Addresses on co.ContactID equals ad.AddressID
                               join ph in db.Phones on ad.ContactID equals ph.ContactID
                               where co.ContactID == id
                               select new Models.Contact
                               {
                                   id = co.ContactID,
                                   name = new Models.Name
                                   {
                                       first = na.First,
                                       middle = na.Middle,
                                       last = na.Last
                                   },
                                   address = new Models.Address
                                   {
                                       street = ad.Street,
                                       city = ad.City,
                                       state = ad.State,
                                       zip = ad.Zip
                                   },

                                   email = co.Email
                               }).First();

                    contact.phone = (from p in db.Phones
                                     where p.ContactID == id
                                     select new Models.Phone
                                     {
                                         phoneid = p.PhoneID,
                                         number = p.Number,
                                         type = p.Type.ToString()
                                     }).ToList();
                }

                if (contact == null) return NotFound();

                return Ok(contact);
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
        }

        /// <summary>
        /// Get a call list
        /// </summary>
        /// <returns></returns>
        // GET api/contacts/call-list
        [HttpGet]
        [Route("api/Contacts/call-list")]
        public IHttpActionResult GetCallList()
        {
            try
            {
                using (var db = new ExerciseEntities())
                {
                    List<Models.CallListItem.Contact> list = (from co in db.Contacts
                                                              join na in db.Names on co.ContactID equals na.ContactID
                                                              join ph in db.Phones on na.ContactID equals ph.ContactID
                                                              where ph.Type == "home"
                                                              orderby na.Last, na.First
                                                              select new Models.CallListItem.Contact
                                                              {
                                                                  name = new Models.Name
                                                                  {
                                                                      first = na.First,
                                                                      middle = na.Middle,
                                                                      last = na.Last
                                                                  },
                                                                  phone = ph.Number
                                                              }).ToList();

                    return Ok(list);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
        }

        /// <summary>
        /// Create a new contact
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        // POST api/contacts
        [HttpPost]
        [Route("api/Contacts")]
        public IHttpActionResult Post([FromBody] Models.Contact contact)
        {
            if (!ModelState.IsValid) return BadRequest("Not A Valid Contact.");

            try
            {
                using (var db = new ExerciseEntities())
                {
                    var con = new Contact() 
                    {
                        Email = contact.email
                    };

                    db.Contacts.Add(con);
                    db.SaveChanges();
                    int id = con.ContactID;

                    db.Names.Add(new Name
                    {
                        ContactID = id,
                        First = contact.name.first,
                        Middle = contact.name.middle,
                        Last = contact.name.last
                    });

                    db.Addresses.Add(new Entities.Address
                    {
                        ContactID = id,
                        Street = contact.address.street,
                        City = contact.address.city,
                        State = contact.address.state,
                        Zip = contact.address.zip
                    });

                    foreach (Models.Phone p in contact.phone)
                    {
                        Entities.Phone ph = new Entities.Phone();
                        ph.ContactID = id;
                        ph.Number = p.number;
                        ph.Type = p.type;

                        db.Phones.Add(ph);
                    }

                    db.SaveChanges();

                    return Content(HttpStatusCode.Created, contact);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }

        }

        /// <summary>
        /// Update a contact
        /// </summary>
        /// <param name="id"></param>
        /// <param name="contact"></param>
        /// <returns></returns>
        // PUT api/contacts/5
        [HttpPut]
        [Route("api/Contacts/{id}")]
        public IHttpActionResult Put(int id, [FromBody] Models.Contact contact)
        {
            if (!ModelState.IsValid) return BadRequest("Not a valid contact");

            try
            {
                using (var db = new ExerciseEntities())
                {
                    var con = db.Contacts.Where(x => x.ContactID == id).FirstOrDefault<Contact>();

                    if (con != null)
                    {
                        con.Email = contact.email;
                        db.SaveChanges();
                    }

                    var nam = db.Names.Where(x => x.ContactID == id).FirstOrDefault<Name>();

                    if (nam != null)
                    {
                        nam.First = contact.name.first;
                        nam.Middle = contact.name.middle;
                        nam.Last = contact.name.last;
                        db.SaveChanges();
                    }

                    var add = db.Addresses.Where(x => x.ContactID == id).FirstOrDefault<Address>();

                    if (add != null)
                    {
                        add.Street = contact.address.street;
                        add.City = contact.address.city;
                        add.State = contact.address.state;
                        add.Zip = contact.address.zip;
                        db.SaveChanges();
                    }

                    var pho = db.Phones.Where(x => x.ContactID == id).ToList();

                    if (pho != null)
                    {
                        foreach (var item in pho)
                        {
                            foreach (var item2 in contact.phone)
                            {
                                if (item.PhoneID == item2.phoneid)
                                {
                                    item.Number = item2.number;
                                    item.Type = item2.type;
                                }
                            }
                        }
                    }

                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return InternalServerError();
            }
        }

        /// <summary>
        /// Delete a contact
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // DELETE api/contacts/5
        [HttpDelete]
        [Route("api/Contacts/{id}")]
        public IHttpActionResult Delete(int id)
        {
            if (id <= 0) return NotFound();

            using (var db = new ExerciseEntities())
            {
                db.Contacts.Remove(db.Contacts.FirstOrDefault(x => x.ContactID == id));
                db.Names.Remove(db.Names.FirstOrDefault(x => x.ContactID == id));
                db.Addresses.Remove(db.Addresses.FirstOrDefault(x => x.ContactID == id));
                db.Phones.RemoveRange(db.Phones.Where(x => x.ContactID == id));
                db.SaveChanges();

                return Ok();
            }
        }
    }
}
