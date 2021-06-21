using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.App.Stock.Report;

namespace StockRptChange
{
    public class CustomerReportService : StockClassStaRpt
    {
        //定义临时表数组
        private string[] _customRptTempDt;

        /// <summary>
        /// 作用:1）创建自定义临时表并保存初步查询结果 2)将第1步提出的查询结果与新SQL结合并INTO至基类默认的临时表
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="tableName">最后输出的临时表</param>
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            //创建临时表,用于存放自已的数据
            var dbservice = ServiceHelper.GetService<IDBService>();
            _customRptTempDt = dbservice.CreateTemporaryTableName(Context, 1);
            var strDt = _customRptTempDt[0];

            //调用基类的方法,获取初步的查询结果赋值到临时表
            base.BuilderReportSqlAndTempTable(filter, strDt);

            //对初步的查询结果进行处理,然后写回基类默认的存放查询结果的临时表
            //var sb=new StringBuilder();
            var strSql = $@"
                             SELECT T1.*,C.FDATAVALUE FTYPE,   --品牌
	                                E.FDATAVALUE FFEN,    --分类
	                                G.FDATAVALUE FBI,     --品类
	                                i.FDATAVALUE FZHU,    --组份
	                                x1.FDATAVALUE FCHANG, --常规/订制
	                                y1.FDATAVALUE FZHUJI, --阻击产品
	                                Z1.FDATAVALUE FXI,    --系数
	                                XX1.FDATAVALUE FCOL,  --水性油性
	                                YY1.FDATAVALUE FYUAN, --原漆半成品属性
	                                ZZ1.FDATAVALUE FYAN   --研发类别
                             INTO {tableName}
                             FROM {strDt} T1 /*后台'物料收发分类统计表'临时表*/
                             INNER JOIN T_BD_MATERIAL a ON T1.FMATERIALID=a.FMATERIALID
                             
                             LEFT JOIN T_BAS_ASSISTANTDATAENTRY b ON a.F_YTC_ASSISTANT7=b.FENTRYID
                             LEFT JOIN dbo.T_BAS_ASSISTANTDATAENTRY_L C ON b.FENTRYID=C.FENTRYID AND C.FLOCALEID=2052  --品牌

                             LEFT JOIN dbo.T_BAS_ASSISTANTDATAENTRY D ON A.F_YTC_ASSISTANT=D.FENTRYID
                             LEFT JOIN T_BAS_ASSISTANTDATAENTRY_L E ON D.FENTRYID=E.FENTRYID AND E.FLOCALEID=2052  --分类

                             LEFT JOIN dbo.T_BAS_ASSISTANTDATAENTRY F ON A.F_YTC_ASSISTANT1=F.FENTRYID
                             LEFT JOIN dbo.T_BAS_ASSISTANTDATAENTRY_L G ON F.FENTRYID=G.FENTRYID AND G.FLOCALEID=2052 --品类

                             LEFT JOIN dbo.T_BAS_ASSISTANTDATAENTRY H ON A.F_YTC_ASSISTANT11=H.FENTRYID
                             LEFT JOIN dbo.T_BAS_ASSISTANTDATAENTRY_L I ON H.FENTRYID=I.FENTRYID AND I.FLOCALEID=2052 --组份

                             LEFT JOIN dbo.T_BAS_ASSISTANTDATAENTRY X0 ON A.F_YTC_ASSISTANT1111=X0.FENTRYID
                             LEFT JOIN dbo.T_BAS_ASSISTANTDATAENTRY_L X1 ON X0.FENTRYID=X1.FENTRYID AND X1.FLOCALEID=2052 --常规/订制

                             LEFT JOIN T_BAS_ASSISTANTDATAENTRY Y0 ON A.F_YTC_ASSISTANT11111=Y0.FENTRYID
                             LEFT JOIN dbo.T_BAS_ASSISTANTDATAENTRY_L Y1 ON Y0.FENTRYID=Y1.FENTRYID AND Y1.FLOCALEID=2052  --阻击产品

                             LEFT JOIN T_BAS_ASSISTANTDATAENTRY Z0 ON A.F_YTC_ASSISTANT2=Z0.FENTRYID
                             LEFT JOIN T_BAS_ASSISTANTDATAENTRY_L Z1 ON Z0.FENTRYID=Z1.FENTRYID AND Z1.FLOCALEID=2052    --系数

                             LEFT JOIN T_BAS_ASSISTANTDATAENTRY XX ON A.F_YTC_ASSISTANT3=XX.FENTRYID
                             LEFT JOIN T_BAS_ASSISTANTDATAENTRY_L XX1 ON XX.FENTRYID=XX1.FENTRYID AND XX1.FLOCALEID=2052 --水性油性

                             LEFT JOIN T_BAS_ASSISTANTDATAENTRY YY ON A.F_YTC_ASSISTANT31=YY.FENTRYID
                             LEFT JOIN T_BAS_ASSISTANTDATAENTRY_L YY1 ON YY.FENTRYID=YY1.FENTRYID AND YY1.FLOCALEID=2052  --原漆半成品属性

                             LEFT JOIN T_BAS_ASSISTANTDATAENTRY ZZ ON A.F_YTC_ASSISTANT8=ZZ.FENTRYID
                             LEFT JOIN T_BAS_ASSISTANTDATAENTRY_L ZZ1 ON ZZ.FENTRYID=ZZ1.FENTRYID AND ZZ1.FLOCALEID=2052   --研发类别
                         ";
            //sb.AppendFormat(strSql, tableName, strDt);
            DBUtils.Execute(Context, strSql);//sb.ToString()
        }

        /// <summary>
        /// 重写报表动态列
        /// (注:当在BOS里新增列没有生效时使用;若重写此方法在运行后只查看到新增列而没有原来的列,就不能重写此方法,需将新列在BOS里新增)
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            var reportHeader = base.GetReportHeaders(filter);
            //添加新增列
            reportHeader.AddChild("FTYPE", new LocaleValue("品牌", this.Context.LogLocale.LCID));
            reportHeader.AddChild("FFEN", new LocaleValue("分类", this.Context.LogLocale.LCID));
            reportHeader.AddChild("FBI", new LocaleValue("品类", this.Context.LogLocale.LCID));
            reportHeader.AddChild("FZHU", new LocaleValue("组份", this.Context.LogLocale.LCID));
            reportHeader.AddChild("FCHANG", new LocaleValue("常规订制", this.Context.LogLocale.LCID));
            reportHeader.AddChild("FZHUJI", new LocaleValue("阻击产品", this.Context.LogLocale.LCID));
            reportHeader.AddChild("FXI", new LocaleValue("系数", this.Context.LogLocale.LCID));
            reportHeader.AddChild("FCOL", new LocaleValue("水性油性", this.Context.LogLocale.LCID));
            reportHeader.AddChild("FYUAN", new LocaleValue("原漆半成品属性", this.Context.LogLocale.LCID));
            reportHeader.AddChild("FYAN", new LocaleValue("研发类别", this.Context.LogLocale.LCID));
            return reportHeader;
        }

        /// <summary>
        /// 关闭报表时执行
        /// </summary>
        public override void CloseReport()
        {
            //删除临时表
            if (_customRptTempDt.IsNullOrEmptyOrWhiteSpace())
            {
                return;
            }
            var dbService = ServiceHelper.GetService<IDBService>();
            //使用后的临时表删除
            dbService.DeleteTemporaryTableName(Context, _customRptTempDt);

            base.CloseReport();
        }
    }
}
