using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Dochazka.Data;
using Dochazka.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Dochazka.Authorization;

namespace Dochazka.Controllers
{
    public class ContactsController : DI_BaseController
    {
        private readonly ILogger<ContactsController> _logger;

        public ContactsController(
            ILogger<ContactsController> logger,
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager)
            : base(context, authorizationService, userManager)
        {
            _logger = logger;
        }

        // GET: Contacts
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var contacts = from c in Context.Contact
                           select c;

            var isAuthorized = User.IsInRole(Constants.ContactManagersRole) ||
                               User.IsInRole(Constants.ContactAdministratorsRole);

            var currentUserId = UserManager.GetUserId(User);

            // Only approved contacts are shown UNLESS you're authorized to see them
            // or you are the owner.
            if (!isAuthorized)
            {
                contacts = contacts.Where(c => c.Status == ContactStatus.Approved
                                            || c.OwnerID == currentUserId);
            }

            return View(await contacts.ToListAsync());
        }

        // GET: Contacts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }           

            var contact = await Context.Contact
                .FirstOrDefaultAsync(m => m.ContactId == id);

            if (contact == null)
            {
                return NotFound();
            }
            var isAuthorized = User.IsInRole(Constants.ContactManagersRole) ||
                   User.IsInRole(Constants.ContactAdministratorsRole);
            
            var currentUserId = UserManager.GetUserId(User);

            if (!isAuthorized
                && currentUserId != contact.OwnerID
                && contact.Status != ContactStatus.Approved)
            {
                return Forbid();
            }

            return View(contact);
        }

        // POST: Contacts/Details/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(int id, ContactStatus status)
        {
            var contact = await Context.Contact.FirstOrDefaultAsync(
                                          m => m.ContactId == id);
            if (contact == null)
            {
                return NotFound();
            }
            var contactOperation = (status == ContactStatus.Approved)
                                           ? ContactOperations.Approve
                                           : ContactOperations.Reject;
            
            var isAuthorized = await AuthorizationService.AuthorizeAsync(User, contact,
                            contactOperation);
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }
            contact.Status = status;
            Context.Contact.Update(contact);
            await Context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Contacts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Contacts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContactId,Name,Address,City,State,Zip,Email")] Contact contact)
        {

            if (!ModelState.IsValid)
            {
                return View(contact);
            }

            contact.OwnerID = UserManager.GetUserId(User);

            // requires using ContactManager.Authorization;
            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                                        User, contact,
                                                        ContactOperations.Create);

            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            Context.Add(contact);
            await Context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Contacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await Context.Contact.FindAsync(id);
            if (contact == null)
            {
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                          User, contact,
                                          ContactOperations.Update);

            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            return View(contact);
        }

        // POST: Contacts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, byte[] rowVersion)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Fetch Contact from DB to get OwnerID.
            var contactToUpdate = await Context
                .Contact
                .FirstOrDefaultAsync(m => m.ContactId == id);

            if (contactToUpdate == null)
            {
                Contact deletedContact = new Contact();
                await TryUpdateModelAsync(deletedContact);
                ModelState.AddModelError(string.Empty,
                    "Unable to save changes. The contact was deleted by another user.");                
                return View(deletedContact);
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                         User, contactToUpdate,
                                         ContactOperations.Update);

            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            Context.Entry(contactToUpdate).Property("RowVersion").OriginalValue = rowVersion;

            if (await TryUpdateModelAsync<Contact>(
                contactToUpdate,
                "",
                s => s.Name, s => s.Address, s => s.City, s => s.State, s => s.Zip, s => s.Email))
            {                
                //contact.OwnerID = originalContact.OwnerID;

                // Mark contact entity state as Modified, so that it can be updated in DB
                //Context.Attach(contact).State = EntityState.Modified;

                if (contactToUpdate.Status == ContactStatus.Approved)
                {
                    // If the contact is updated after approval, 
                    // and the user cannot approve,
                    // set the status back to submitted so the update can be
                    // checked and approved.
                    var canApprove = await AuthorizationService.AuthorizeAsync(User,
                                            contactToUpdate,
                                            ContactOperations.Approve);

                    if (!canApprove.Succeeded)
                    {
                        contactToUpdate.Status = ContactStatus.Submitted;
                    }
                }

                try
                {
                    //Context.Update(contact);
                    await Context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (Contact)exceptionEntry.Entity;
                    var databaseEntry = exceptionEntry.GetDatabaseValues();
                    if (databaseEntry == null)
                    {
                        ModelState.AddModelError(string.Empty,
                            "Unable to save changes. The Contact was deleted by another user.");
                    }
                    else
                    {
                        var databaseValues = (Contact)databaseEntry.ToObject();

                        if (databaseValues.Name != clientValues.Name)
                        {
                            ModelState.AddModelError("Name", $"Current value: {databaseValues.Name}");
                        }
                        if (databaseValues.Address != clientValues.Address)
                        {
                            ModelState.AddModelError("Address", $"Current value: {databaseValues.Address}");
                        }
                        if (databaseValues.City != clientValues.City)
                        {
                            ModelState.AddModelError("City", $"Current value: {databaseValues.City}");
                        }
                        if (databaseValues.State != clientValues.State)
                        {
                            ModelState.AddModelError("State", $"Current value: {databaseValues.State}");
                        }
                        if (databaseValues.Zip != clientValues.Zip)
                        {
                            ModelState.AddModelError("Zip", $"Current value: {databaseValues.Zip}");
                        }
                        if (databaseValues.Email != clientValues.Email)
                        {
                            ModelState.AddModelError("Email", $"Current value: {databaseValues.Email}");
                        }

                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                                + "was modified by another user after you got the original value. The "
                                + "edit operation was canceled and the current values in the database "
                                + "have been displayed. If you still want to edit this record, click "
                                + "the Save button again. Otherwise click the Back to List hyperlink.");
                        contactToUpdate.RowVersion = (byte[])databaseValues.RowVersion;
                        ModelState.Remove("RowVersion");
                    }
                }                                      
            }
            return View(contactToUpdate);
        }

        // GET: Contacts/Delete/5
        public async Task<IActionResult> Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await Context.Contact
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ContactId == id);
            if (contact == null)
            {
                if (concurrencyError.GetValueOrDefault())
                {
                    return RedirectToAction(nameof(Index));
                }
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                         User, contact,
                                         ContactOperations.Delete);
            
            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }

            if (concurrencyError.GetValueOrDefault())
            {
                ViewData["ConcurrencyErrorMessage"] = "The record you attempted to delete "
                    + "was modified by another user after you got the original values. "
                    + "The delete operation was canceled and the current values in the "
                    + "database have been displayed. If you still want to delete this "
                    + "record, click the Delete button again. Otherwise "
                    + "click the Back to List hyperlink.";
            }

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, Contact contact)
        {
            var originalContact = await Context
                .Contact.AsNoTracking()
                .FirstOrDefaultAsync(m => m.ContactId == id);

            if ((id != contact.ContactId) || (originalContact == null))
            {
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                                         User, originalContact,
                                         ContactOperations.Delete);

            if (!isAuthorized.Succeeded)
            {
                return Forbid();
            }           

            try
            {
                Context.Contact.Remove(contact);
                await Context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                //Log the error (uncomment ex variable name and write a log.)
                return RedirectToAction(nameof(Delete), new { id = contact.ContactId, concurrencyError = true });
            }            
        }

        private bool ContactExists(int id)
        {
            return Context.Contact.Any(e => e.ContactId == id);
        }
    }
}
