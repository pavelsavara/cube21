<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BestPath.aspx.cs" Inherits="Server.BestPath" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" />
        <div>
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <table>
                        <tr>
                            <td>
                    <asp:Label ID="Label1" runat="server" Text="Top"></asp:Label>
                            </td>
                            <td>
                    <asp:Label ID="Label2" runat="server" Text="Bottom"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td>
                    <asp:TextBox ID="tbTop" runat="server"></asp:TextBox>
                            </td>
                            <td>
                    <asp:TextBox ID="tbBot" runat="server" ></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2" align="center">
                                <asp:Button ID="btGetPath" runat="server" Text="Get Path" OnClick="btGetPath_Click" />
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2" align="center">
                    <asp:Label ID="lbResult" runat="server"></asp:Label>
                            </td>
                        </tr>
                    </table>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </form>
</body>
</html>
