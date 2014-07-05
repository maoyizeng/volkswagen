using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Volkswagen.Models;

namespace Volkswagen.Controllers
{
    public class EquipmentController : Controller
    {
        //
        // GET: /Equipment/
        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Equipment/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Equipment/Create
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Equipment/Create
        [HttpPost]
        public ActionResult Create([Bind(Include = "CourseID,Title,Credits,DepartmentID")]EquipmentModels em)
        {
            try
            {
                // TODO: Add insert logic here
                if (!CheckEntity())
                {
                    Error();
                }

                InsertToDB(em);

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
            
        }

        //
        // GET: /Equipment/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Equipment/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, [Bind(Include = "CourseID,Title,Credits,DepartmentID")]EquipmentModels em)
        {
            try
            {
                // TODO: Add update logic here
                if (!CheckEntity())
                {
                    Error();
                }

                EditDB(em);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Equipment/Delete/5
        public ActionResult Delete(int id)
        {
            InsertToDB(TableAdmin, id);
            DeleteFromDB(id);
            return View();
        }

        //
        // POST: /Equipment/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
