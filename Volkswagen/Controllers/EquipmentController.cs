using MvcContrib.Pagination;
using MvcContrib.Sorting;
using MvcContrib.UI.Grid;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Volkswagen.DAL;
using Volkswagen.Models;

namespace Volkswagen.Controllers
{

    [UserAuthorized] // 自行编写的权限特性, 当没有登录时跳转到主页
    public class EquipmentController : Controller
    {
        public SVWContext db = new SVWContext();

        private string[] operation = new string[] {
            "=",
            ">",
            "<",
            ">=",
            "<="
        };
        

        // GET: /Equipment/
        // 以get方式调用的主页
        // page: 页码号
        // model: 排序选项(Column, Direction)
        // selected_item: 详细显示的记录主键
        // get方式调用的函数, 得到调用时参数的方法有两种, 一是如下在函数的参数列表里, 二是在Request[]数组里, 都可以使用
        public async Task<ActionResult> Index(int? page, GridSortOptions model, string selected_item)
        {
            // 将之后页面的预设排序/查看值设上
            // ViewData是除了主要model外给页面传值的途径
            ViewData["model"] = model;
            ViewData["selected"] = selected_item;

            // 查询
            IQueryable<EquipmentModels> list = getQuery(false);

            // 如果Column不为空, 说明需要进行排序, 根据Direction进行升序或降序
            if (!string.IsNullOrEmpty(model.Column))
            {
                if (model.Direction == SortDirection.Descending)
                {
                    list = list.OrderBy(model.Column + " desc");
                }
                else
                {
                    list = list.OrderBy(model.Column + " asc");
                }
            }
            
            // 返回页面, 主model为IPagination, 如果page有值就以page值-100返回, 否则就是1-100
            return View(list.ToList().AsPagination(page ?? 1, 100));
        }

        // 以post方式调用主页
        // page: 页码
        // select_item: 要详细显示的记录主键
        // 排序的项在Request.Form[]里可以得到
        // post方式调用的函数, 得到调用时参数的方法有两种, 一是如下在函数的参数列表里, 二是在Request.Form[]数组里, 都可以使用
        [HttpPost]
        public async Task<ActionResult> Index(int? page, string selected_item)
        {
            // 得到排序选项, 并给页面的ViewData赋值
            GridSortOptions model = new GridSortOptions();
            model.Column = Request.Form["Column"];
            model.Direction = (Request.Form["Direction"] == "Ascending") ? SortDirection.Ascending : SortDirection.Descending;
            ViewData["model"] = model;
            ViewData["selected"] = selected_item;
            
            // 查询
            IQueryable<EquipmentModels> list = getQuery();

            // 排序
            if (!string.IsNullOrEmpty(model.Column))
            {
                if (model.Direction == SortDirection.Descending)
                {
                    list = list.OrderBy(model.Column + " desc");
                }
                else
                {
                    list = list.OrderBy(model.Column + " asc");
                }
            }

            // 返回 如果页码 不为空则返回页码-100 否则1-100
            return View(list.ToList().AsPagination(page ?? 1, 100));
        }

        // 查询方法
        // post: 是否是post方式 false表示get
        private IQueryable<EquipmentModels> getQuery(bool post = true)
        {
            // 以下使用Expression类构造出一句数据库的查询语句, 比较难看懂
            // xx.where(p => p.[filedn] [opn] [operandn] && ...)

            // 构造一个参数 p | p是EquipmentModel类型的
            ParameterExpression param = Expression.Parameter(typeof(EquipmentModels), "p");

            // 构造查询的bool条件
            Expression filter = Expression.Constant(true);
            // 逐条得到页面传来的查询行进行分析
            for (int n = 0; ; n++)
            {
                // 根据post还是get 得到各自的列名/操作符/操作数 并且赋值给ViewData以再传回页面
                string field = (post ? Request.Form["field" + n] : Request["field" + n]);
                ViewData["field" + n] = field;
                string op = (post ? Request.Form["op" + n] : Request["op" + n]);
                ViewData["op" + n] = op;
                string operand = (post ? Request.Form["operand" + n] : Request["operand" + n]);
                ViewData["operand" + n] = operand;

                // 如果列名为空 说明查询结束 直接跳出
                if (string.IsNullOrEmpty(field)) break;
                // 如果操作数为空 说明此行查询无效 看下一行
                if (string.IsNullOrEmpty(operand)) continue;


                //p.[filedn] | 在EquipmentModels中得到列名作为param
                Expression left = Expression.Property(param, typeof(EquipmentModels).GetProperty(field));
                //[operandn] | 将操作数构造成常数
                Expression right = Expression.Constant(operand);

                // 当列名的类型不是string时, 需要对操作数进行处理, 以类型匹配
                switch (field)
                {
                    case "WSArea":
                        // enum 进行转换
                        right = Expression.Constant(Convert.ToInt32(Enum.Parse(typeof(EquipmentModels.WSNames), operand)));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "ItemInspect":
                    case "RegularCare":
                    case "Check":
                        right = Expression.Constant(Convert.ToInt32(Enum.Parse(typeof(EquipmentModels.ThereBe), operand)));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "RoutingInspect":
                        right = Expression.Constant(Convert.ToInt32(Enum.Parse(typeof(EquipmentModels.YesNo), operand)));
                        right = Expression.Convert(right, left.Type);
                        break;
                    case "ChangeTime":
                    case "CreateTime":
                        // datetime 转换
                        right = Expression.Constant(Convert.ToDateTime(operand));
                        right = Expression.Convert(right, left.Type);
                        break;
                    default:
                        break;
                }

                Expression result;

                // 根据操作符不同, 将列名和操作数用操作符连接起来构成一句 p.[filedn] [opn] [operandn]
                switch (op)
                {
                    case "0":
                        result = Expression.Equal(left, right);
                        break;
                    case "1":
                        result = Expression.GreaterThan(left, right);
                        break;
                    case "2":
                        result = Expression.LessThan(left, right);
                        break;
                    case "3":
                        result = Expression.GreaterThanOrEqual(left, right);
                        break;
                    case "4":
                        result = Expression.LessThanOrEqual(left, right);
                        break;
                    case "5":
                        result = Expression.NotEqual(left, right);
                        break;
                    case "6": //Contain 包含要通过特别处理 p.[filedn].Contains([operandn])
                        result = Expression.Call(left, typeof(string).GetMethod("Contains", new Type[]{typeof(string)}), right);
                        break;
                    default:
                        result = Expression.Equal(left, right);
                        break;
                }
                filter = Expression.And(filter, result);
            }
            
            // p => p.[filedn] [opn] [operandn] && ...
            Expression pred = Expression.Lambda(filter, param);

            // where(p => p.[filedn] [opn] [operandn] && ...)
            var e = db.Equipments;
            // 构造函数调用
            Expression expr = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(EquipmentModels) }, Expression.Constant(e), pred);

            // 得到结果
            IQueryable<EquipmentModels> list = db.Equipments.AsQueryable().Provider.CreateQuery<EquipmentModels>(expr);
            
            return list;
        }

        // 多选时 分析checkbox状态
        private List<EquipmentModels> getSelected(IQueryable<EquipmentModels> l)
        {
            List<EquipmentModels> list = new List<EquipmentModels>();
            List<EquipmentModels> list_origin = l.ToList();
            foreach (EquipmentModels e in list_origin)
            {
                // 由于checkbox的名字设置为Checked+主键, 所以这边逐一分析
                var ss = Request.Form["Checked" + e.EquipmentID];
                if ((!string.IsNullOrEmpty(ss)) && (ss != "false"))
                {
                    list.Add(e);
                }
            }
            
            return list;
        }

        // GET: /Equipment/Details/5
        // 详细信息页面请求
        public async Task<ActionResult> Details(string id)
        {
            // id为空, 请求无效
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // 得到记录项
            EquipmentModels equipmentmodels = await db.Equipments.FindAsync(id);
            // 记录项为空, 请求无效
            if (equipmentmodels == null)
            {
                return HttpNotFound();
            }
            return View(equipmentmodels);
        }

        // GET: /Equipment/Create
        // 创建页面 这是请求空的创建页面 所以只返回页面 不做任何事
        [Authorize(Roles="Admin")] // 只有Admin允许操作 否则报错 401没有权限
        public ActionResult Create()
        {
            ViewBag.WSNames = new SelectList(Enum.GetValues(typeof(EquipmentModels.WSNames)));
            ViewBag.ThereBe = new SelectList(Enum.GetValues(typeof(EquipmentModels.ThereBe)));
            ViewBag.YesNo = new SelectList(Enum.GetValues(typeof(EquipmentModels.YesNo)));
            return View();
        }

        // POST: /Equipment/Create
        // 创建记录, 这条是要进行创建工作
        // 返回的值是页面上传回的一条完整EquipmentModel记录
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create([Bind(Include="EquipmentID,EquipDes,Person,Section,WSArea,Photo,ItemInspect,ItemInspectNum,RegularCare,RegularCareNum,Check,CheckNum,RoutingInspect,Rules,TechnicFile,TrainingFile,ChangeTime,Changer,CreateTime,Creator,Remark")] EquipmentModels equipmentmodels)
        {
            if (db.Equipments.Find(equipmentmodels.EquipmentID) != null)
            {
                ViewData["valid"] = "exist_key";
                return View(equipmentmodels);
            }
            if (ModelState.IsValid) // 确认传回的值是有效的, 要是有输入错误, 则返回原页面并显示错误
            {
                // 给记录做上必要记录
                equipmentmodels.Changer = User.Identity.Name;
                equipmentmodels.Creator = User.Identity.Name;
                equipmentmodels.CreateTime = DateTime.Now;
                equipmentmodels.ChangeTime = DateTime.Now;
                // 给数据库添加记录, 这是还没有保存, 所以还没真加到数据库里
                db.Equipments.Add(equipmentmodels);

                // 保存数据库改动, 这时x是操作的结果 非零表示成功
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    // 成功时添加历史记录
                    ArEquipmentModels ar = new ArEquipmentModels(equipmentmodels);
                    ar.Operator = ArEquipmentModels.OperatorType.创建;
                    db.ArEquipments.Add(ar);
                    await db.SaveChangesAsync();
                }               
                
                // 返回主页
                return RedirectToAction("Index");
            }

            ViewBag.WSNames = new SelectList(Enum.GetValues(typeof(EquipmentModels.WSNames)));
            ViewBag.ThereBe = new SelectList(Enum.GetValues(typeof(EquipmentModels.ThereBe)));
            ViewBag.YesNo = new SelectList(Enum.GetValues(typeof(EquipmentModels.YesNo)));
            return View(equipmentmodels);
        }

        // GET: /Equipment/Edit/5
        // 编辑页面请求 同Create
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EquipmentModels equipmentmodels = await db.Equipments.FindAsync(id);
            if (equipmentmodels == null)
            {
                return HttpNotFound();
            }
            return View(equipmentmodels);
        }

        // POST: /Equipment/Edit/5
        // 编辑操作请求 同Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit([Bind(Include = "EquipmentID,EquipDes,Person,Section,WSArea,Photo,ItemInspect,ItemInspectNum,RegularCare,RegularCareNum,Check,CheckNum,RoutingInspect,ChangeTime,Changer,CreateTime,Creator,Remark")] EquipmentModels equipmentmodels)
        {
            if (ModelState.IsValid)
            {
                // 得到同主键的原记录
                var toUpdate = db.Equipments.Find(equipmentmodels.EquipmentID);

                // 修改新的记录
                equipmentmodels.Changer = User.Identity.Name;
                equipmentmodels.ChangeTime = DateTime.Now;
                equipmentmodels.Creator = toUpdate.Creator;
                equipmentmodels.CreateTime = toUpdate.CreateTime;
                equipmentmodels.Photo = toUpdate.Photo;

                // 将原纪录剔除
                db.Entry(toUpdate).State = EntityState.Detached;
                // 将新记录加上
                db.Entry(equipmentmodels).State = EntityState.Modified;

                // 同步数据库
                int x = await db.SaveChangesAsync();

                if (x != 0)
                {
                    ArEquipmentModels ar = new ArEquipmentModels(toUpdate);
                    ar.Operator = ArEquipmentModels.OperatorType.修改;
                    db.ArEquipments.Add(ar);
                    await db.SaveChangesAsync();
                }
                
                
                return RedirectToAction("Index");
            }
            return View(equipmentmodels);
        }

        // POST: /Equipment/EditMultiple/
        // 复数编辑
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> EditMultiple()
        {
            IQueryable<EquipmentModels> l = getQuery();
            List<EquipmentModels> list = getSelected(l);
            if (ViewData["list"] == null) ViewData["list"] = list;
            //string key = list.First().EquipmentID;
            //return RedirectToAction("Edit", new { id = key });
            return View("ChangeMultiple", new EquipmentModels());
        }

        // POST: /Equipment/ChangeMultiple/
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ChangeMultiple([Bind(Include = "EquipmentID,EquipDes,Person,Section,WSArea,Photo,ItemInspect,ItemInspectNum,RegularCare,RegularCareNum,Check,CheckNum,RoutingInspect,ChangeTime,Changer,CreateTime,Creator,Remark")] EquipmentModels equipmentmodels)
        {
            bool changed = false;
            List<EquipmentModels> l = new List<EquipmentModels>();
            for (int i = 0; ; i++)
                {
                    string id = Request.Form["item" + i];
                    if (Request.Form["item" + i] == null) break;
                    EquipmentModels e = db.Equipments.Find(id);
                    l.Add(e);
                    ArEquipmentModels ar = new ArEquipmentModels(e);
                    // 如果得到的值不为空, 说明需要进行修改
                    if (equipmentmodels.EquipDes != null && ModelState.IsValidField("EquipDes")) e.EquipDes = equipmentmodels.EquipDes;
                    if (equipmentmodels.Person != null  && ModelState.IsValidField("Person")) e.Person = equipmentmodels.Person;
                    if (equipmentmodels.Section != null  && ModelState.IsValidField("Section")) e.Section = equipmentmodels.Section;
                    if (equipmentmodels.WSArea != null  && ModelState.IsValidField("WSArea")) e.WSArea = equipmentmodels.WSArea;
                    if (equipmentmodels.ItemInspect != null  && ModelState.IsValidField("ItemInspect")) e.ItemInspect = equipmentmodels.ItemInspect;
                    if (equipmentmodels.ItemInspectNum != null && ModelState.IsValidField("ItemInspectNum")) e.ItemInspectNum = equipmentmodels.ItemInspectNum;
                    if (equipmentmodels.RegularCare != null  && ModelState.IsValidField("RegularCare")) e.RegularCare = equipmentmodels.RegularCare;
                    if (equipmentmodels.RegularCareNum != null && ModelState.IsValidField("RegularCareNum")) e.RegularCareNum = equipmentmodels.RegularCareNum;
                    if (equipmentmodels.Check != null  && ModelState.IsValidField("Check")) e.Check = equipmentmodels.Check;
                    if (equipmentmodels.CheckNum != null && ModelState.IsValidField("CheckNum")) e.CheckNum = equipmentmodels.CheckNum;
                    if (equipmentmodels.RoutingInspect != null  && ModelState.IsValidField("RoutingInspect")) e.RoutingInspect = equipmentmodels.RoutingInspect;
                    if (equipmentmodels.Remark != null  && ModelState.IsValidField("Remark")) e.Remark = equipmentmodels.Remark;
                    if (db.Entry(e).State == EntityState.Modified )
                    {
                        e.Changer = User.Identity.Name;
//                      e.Creator = User.Identity.Name;
//                      e.CreateTime = DateTime.Now;
                        e.ChangeTime = DateTime.Now;
                        int x = await db.SaveChangesAsync();
                        if (x != 0)
                        {      
                            changed = true;
                            ar.Operator = ArEquipmentModels.OperatorType.修改;
                            db.ArEquipments.Add(ar);
                            await db.SaveChangesAsync();
                        }                       
                    }
                }
            if (changed)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ViewData["list"] = l;
                return View(new EquipmentModels());
            }
                
           
        }

        // GET: /Equipment/Delete/5
        // 删除 这条是显示要删除的项 并不做删除操作
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EquipmentModels equipmentmodels = await db.Equipments.FindAsync(id);
            if (equipmentmodels == null)
            {
                return HttpNotFound();
            }
            return View(equipmentmodels);
        }

        // POST: /Equipment/Delete/5
        // 确认删除 做删除操作
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            EquipmentModels toDelete = await db.Equipments.FindAsync(id);
            // 先创建历史记录, 因为remove后toDelete会被修改
            ArEquipmentModels ar = new ArEquipmentModels(toDelete);
            // 删除, 还未同步数据库, 并未真正删除
            db.Equipments.Remove(toDelete);
            
            int x = await db.SaveChangesAsync();
            if (x != 0){
                // 如果删除 添加历史记录
                ar.Operator = ArEquipmentModels.OperatorType.删除;
                db.ArEquipments.Add(ar);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // POST: /Equipment/DeleteMultiple/
        // 复数删除
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteMultiple(int? page, string selected_item)
        {
            IQueryable<EquipmentModels> l = getQuery();
            List<EquipmentModels> list = getSelected(l);
            foreach (EquipmentModels e in list)
            {
                ArEquipmentModels ar = new ArEquipmentModels(e);
                db.Equipments.Remove(e);
                int x = await db.SaveChangesAsync();
                if (x != 0)
                {
                    ar.Operator = ArEquipmentModels.OperatorType.删除;
                    db.ArEquipments.Add(ar);
                    await db.SaveChangesAsync();
                }
            }
            GridSortOptions model = new GridSortOptions();
            model.Column = Request.Form["Column"];
            model.Direction = (Request.Form["Direction"] == "Ascending") ? SortDirection.Ascending : SortDirection.Descending;
            ViewData["model"] = model;
            ViewData["selected"] = selected_item;
            l = getQuery();

            if (!string.IsNullOrEmpty(model.Column))
            {
                if (model.Direction == SortDirection.Descending)
                {
                    l = l.OrderBy(model.Column + " desc");
                }
                else
                {
                    l = l.OrderBy(model.Column + " asc");
                }
            }
            return View("Index", l.ToList().AsPagination(page ?? 1, 100));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // POST: /Equipment/FileRemove
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult FileRemove()
        {
            string key = Request.Form["key2"];

            EquipmentModels e = db.Equipments.Find(key);
            if (e == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string origin_photo = e.Photo;
            string new_photo = "";
            string[] sArray = Regex.Split(origin_photo, "[$]");

            for (int i = 1; i < sArray.Length; i++)
            {
                if (Request.Form["photo_" + i] != "on")
                {
                    new_photo += "$" + sArray[i];
                }
            }

            ArEquipmentModels ar = new ArEquipmentModels(e);
            // TODO - check e;
            e.Changer = User.Identity.Name;
            e.ChangeTime = DateTime.Now;
            e.Photo = new_photo;
            int x = db.SaveChanges();

            if (x != 0)
            {
                ar.Operator = ArEquipmentModels.OperatorType.修改;
                db.ArEquipments.Add(ar);
                db.SaveChanges();
            }
            return RedirectToAction("Edit", new { id = key });
        }

        // POST: /Equipment/FileUpload
        // 文件上传
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult FileUpload(HttpPostedFileBase[] photos)
        {
            // 得到主键号
            string key = Request.Form["key"];
            string fullname = "";
            string directory = AppDomain.CurrentDomain.BaseDirectory + @"img\equipments\";

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
                
            // 得到多个图片的文件
            foreach (HttpPostedFileBase file in photos)
            {
                if (file != null)
                {
                    string filename = DateTime.Now.ToString("yy-MM-dd HH-mm-ss") + " - " + Path.GetFileName(file.FileName);
                    // 构造需要保存的路径并保存, 然后将$...加到字符串中
                    string filePath = Path.Combine(directory, filename);
                    file.SaveAs(filePath);
                    fullname += "$" + filename;
                }
            }

            EquipmentModels e = db.Equipments.Find(key);
            ArEquipmentModels ar = new ArEquipmentModels(e);
            // TODO - check e;
            e.Changer = User.Identity.Name;
            e.ChangeTime = DateTime.Now;
            e.Photo += fullname;
            int x = db.SaveChanges();

            if (x != 0)
            {
                ar.Operator = ArEquipmentModels.OperatorType.修改;
                db.ArEquipments.Add(ar);
                db.SaveChanges();
            }
            return RedirectToAction("Edit", new { id = key });
            
        }

        // GET: /Equipment/ExportExcel
        // 导出excel 使用StringBuilder的方法, 用html语言输出 与生成报表的方法不一样
        public FileResult ExportExcel()
        {
            var sbHtml = new StringBuilder();

            GridSortOptions model = new GridSortOptions();
            model.Column = Request.Form["Column"];
            model.Direction = (Request.Form["Direction"] == "Ascending") ? SortDirection.Ascending : SortDirection.Descending;

            var l = getQuery();

            // 排序
            if (!string.IsNullOrEmpty(model.Column))
            {
                if (model.Direction == SortDirection.Descending)
                {
                    l = l.OrderBy(model.Column + " desc");
                }
                else
                {
                    l = l.OrderBy(model.Column + " asc");
                }
            }

            var list = l.ToList();

            // 表格
            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            // 头行
            sbHtml.Append("<tr>");
            var lstTitle = new List<string> { 
                "设备编号",
                "设备名称",
                "负责人",
                "所在工段",
                "车间生产线",
                "点检",
                "日常保养",
                "巡检",
                "需更新否",
                "最后修改时间",
                "修改人",
                "创建时间",
                "创建人",
                "备注" };
            foreach (var item in lstTitle)
            {
                sbHtml.AppendFormat("<td style='font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25'>{0}</td>", item);
            }
            sbHtml.Append("</tr>");

            // 为每一行输出
            string format = "<td style='font-size: 12px;height:20px;'>{0}</td>"; // 表格内容格式
            foreach (var i in list)
            {
                sbHtml.Append("<tr>");
                sbHtml.AppendFormat(format, i.EquipmentID);
                sbHtml.AppendFormat(format, i.EquipDes);
                sbHtml.AppendFormat(format, i.Person);
                sbHtml.AppendFormat(format, i.Section);
                sbHtml.AppendFormat(format, i.WSArea);
                sbHtml.AppendFormat(format, i.ItemInspect);
                sbHtml.AppendFormat(format, i.ItemInspectNum);
                sbHtml.AppendFormat(format, i.RegularCare);
                sbHtml.AppendFormat(format, i.RegularCareNum);
                sbHtml.AppendFormat(format, i.Check);
                sbHtml.AppendFormat(format, i.CheckNum);
                sbHtml.AppendFormat(format, i.RoutingInspect);
                // TODO - photo?
                sbHtml.AppendFormat(format, i.ChangeTime);
                sbHtml.AppendFormat(format, i.Changer);
                sbHtml.AppendFormat(format, i.CreateTime);
                sbHtml.AppendFormat(format, i.Creator);
                sbHtml.AppendFormat(format, i.Remark);
                sbHtml.Append("</tr>");
            }
            sbHtml.Append("</table>");
            
            // 转换成二进制
            byte[] fileContents = Encoding.UTF8.GetBytes(sbHtml.ToString());
            
            // 写入输出流
            var fileStream = new MemoryStream(fileContents);
            // 将输出流输出
            return File(fileStream, "application/ms-excel", "设备履历.xls");
        }
    }
}
