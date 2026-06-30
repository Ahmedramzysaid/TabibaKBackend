namespace BusinessLayer;

public static class EmailTemplates
{

    private static string RenderCodeDigits(string code)
    {
        var safeCode = System.Net.WebUtility.HtmlEncode(code ?? "");
        return $@"<table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin:28px 0""><tr><td align=""center"">
<div style=""display:inline-block;padding:16px 40px;font-family:'Courier New',Consolas,monospace;font-size:32px;font-weight:800;letter-spacing:12px;color:#0f766e;background:linear-gradient(180deg,#f0fdfa 0%,#ccfbf1 100%);border:2px solid #0d9488;border-radius:12px"">{safeCode}</div>
</td></tr></table>";
    }

    private static string RenderSocialLinks()
    {
        return @"<div style=""margin-top:16px;"">
      <a href=""https://www.facebook.com/profile.php?id=61582188380371"" target=""_blank"" style=""text-decoration:none;margin:0 6px;"">
        <img src=""https://img.icons8.com/ios-filled/48/0f766e/facebook-new.png"" width=""24"" height=""24"" alt=""Facebook"" style=""display:inline-block; border:none;"" />
      </a>
      <a href=""https://www.instagram.com/tabibak.clinic.official/"" target=""_blank"" style=""text-decoration:none;margin:0 6px;"">
        <img src=""https://img.icons8.com/ios-filled/48/0f766e/instagram-new--v1.png"" width=""24"" height=""24"" alt=""Instagram"" style=""display:inline-block; border:none;"" />
      </a>
      <a href=""https://x.com/TabibakClinic"" target=""_blank"" style=""text-decoration:none;margin:0 6px;"">
        <img src=""https://img.icons8.com/ios-filled/48/0f766e/twitterx.png"" width=""24"" height=""24"" alt=""X"" style=""display:inline-block; border:none;"" />
      </a>
      <a href=""https://www.linkedin.com/company/112224736"" target=""_blank"" style=""text-decoration:none;margin:0 6px;"">
        <img src=""https://img.icons8.com/ios-filled/48/0f766e/linkedin.png"" width=""24"" height=""24"" alt=""LinkedIn"" style=""display:inline-block; border:none;"" />
      </a>
    </div>";
    }

    public static string WelcomePatient(string displayName)
    {
        var name = System.Net.WebUtility.HtmlEncode(
            string.IsNullOrWhiteSpace(displayName) ? "there" : displayName.Trim());

        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width,initial-scale=1"">
<meta name=""x-apple-disable-message-reformatting"">
<title>Welcome to Tabibak</title>
<!--[if mso]>
<style>table {{border-collapse:collapse;}} td {{font-family:Arial,sans-serif;}}</style>
<![endif]-->
</head>
<body style=""margin:0;padding:0;word-spacing:normal;background-color:#f4f7f6;"">
<div role=""article"" aria-roledescription=""email"" lang=""en"" style=""text-size-adjust:100%;-webkit-text-size-adjust:100%;background-color:#f4f7f6;"">
  <table role=""presentation"" style=""width:100%;border:none;border-spacing:0;background-color:#f4f7f6;"" cellpadding=""0"" cellspacing=""0"">
    <tr>
      <td align=""center"" style=""padding:40px 10px;"">
        <table role=""presentation"" style=""width:100%;max-width:520px;border:none;border-spacing:0;font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif;"" cellpadding=""0"" cellspacing=""0"">
          <!-- Green Header -->
          <tr>
            <td align=""center"" style=""background-color:#0f766e;padding:40px 20px;border-radius:16px 16px 0 0;"">
              <table role=""presentation"" style=""border:none;border-spacing:0;"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td align=""center"">
                    <div style=""width:60px;height:60px;background-color:#115e59;border-radius:50%;line-height:60px;font-size:28px;text-align:center;margin-bottom:16px;border:2px solid #0d9488;"">
                        <span style=""color:#ffffff;"">👋</span>
                    </div>
                  </td>
                </tr>
                <tr>
                  <td align=""center"">
                    <h1 style=""margin:0;color:#ffffff;font-size:22px;font-weight:700;"">Welcome to Tabibak</h1>
                    <p style=""margin:6px 0 0 0;color:#ccfbf1;font-size:15px;"">Your journey to better health starts now</p>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
          <!-- White Card Area -->
          <tr>
            <td align=""center"" style=""background-color:#0f766e;padding:0 16px;"">
              <table role=""presentation"" style=""width:100%;border:none;border-spacing:0;background-color:#ffffff;border-radius:12px;box-shadow:0 6px 16px rgba(0,0,0,0.1);"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td align=""left"" style=""padding:36px 32px;color:#334155;font-size:15px;line-height:1.6;"">
                    <p style=""margin:0 0 16px 0;"">Hello <strong>{name}</strong>,</p>
                    <p style=""margin:0 0 24px 0;"">We're thrilled to have you on board! Tabibak connects you with trusted healthcare professionals so you can focus on what matters most &mdash; your health.</p>
                    
                    <p style=""margin:0 0 12px 0;font-weight:600;color:#0f766e;"">Here's what you can do:</p>
                    <table role=""presentation"" style=""width:100%;border:none;border-spacing:0;margin-bottom:24px;"" cellpadding=""0"" cellspacing=""0"">
                      <tr>
                        <td style=""padding:12px 0;border-bottom:1px solid #f1f5f9;width:30px;font-size:18px;"">📅</td>
                        <td style=""padding:12px 0;border-bottom:1px solid #f1f5f9;font-weight:600;color:#334155;width:120px;"">Appointments</td>
                        <td style=""padding:12px 0;border-bottom:1px solid #f1f5f9;color:#64748b;"">Book and manage visits in seconds</td>
                      </tr>
                      <tr>
                        <td style=""padding:12px 0;border-bottom:1px solid #f1f5f9;font-size:18px;"">📋</td>
                        <td style=""padding:12px 0;border-bottom:1px solid #f1f5f9;font-weight:600;color:#334155;"">Records</td>
                        <td style=""padding:12px 0;border-bottom:1px solid #f1f5f9;color:#64748b;"">Access your medical history anytime</td>
                      </tr>
                      <tr>
                        <td style=""padding:12px 0;font-size:18px;"">💬</td>
                        <td style=""padding:12px 0;font-weight:600;color:#334155;"">Care Team</td>
                        <td style=""padding:12px 0;color:#64748b;"">Stay connected with your doctors</td>
                      </tr>
                    </table>

                    <p style=""margin:0 0 16px 0;"">Sign in with your phone number and password to get started.</p>
                    <p style=""margin:0;font-weight:600;color:#0d9488;"">Welcome aboard! 🎉</p>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
          <!-- Bottom trim -->
          <tr><td style=""background-color:#0f766e;height:24px;border-radius:0 0 16px 16px;""></td></tr>
          <!-- Footer -->
          <tr>
            <td align=""center"" style=""padding:32px 20px;"">
              <h2 style=""margin:0;color:#0d9488;font-size:18px;font-weight:700;"">Tabibak</h2>
              <p style=""margin:4px 0 0 0;color:#64748b;font-size:14px;"">Your health, our priority</p>
              {RenderSocialLinks()}
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</div>
</body>
</html>";
    }

    public static string WelcomeDoctor(string displayName)
    {
        var name = System.Net.WebUtility.HtmlEncode(
            string.IsNullOrWhiteSpace(displayName) ? "there" : displayName.Trim());

        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width,initial-scale=1"">
<meta name=""x-apple-disable-message-reformatting"">
<title>Welcome to Tabibak</title>
<!--[if mso]>
<style>table {{border-collapse:collapse;}} td {{font-family:Arial,sans-serif;}}</style>
<![endif]-->
<style>
  @media only screen and (max-width: 500px) {{
    .mobile-stack {{ display: block !important; width: 100% !important; box-sizing: border-box !important; margin-bottom: 12px !important; }}
    .mobile-pad {{ padding: 24px 16px !important; }}
    .rec-card {{ padding: 16px 14px !important; }}
  }}
</style>
</head>
<body style=""margin:0;padding:0;word-spacing:normal;background-color:#f0fdf9;"">
<div role=""article"" aria-roledescription=""email"" lang=""en"" style=""text-size-adjust:100%;-webkit-text-size-adjust:100%;background-color:#f0fdf9;"">
  <table role=""presentation"" style=""width:100%;border:none;border-spacing:0;background-color:#f0fdf9;"" cellpadding=""0"" cellspacing=""0"">
    <tr>
      <td align=""center"" style=""padding:40px 10px;"">
        <table role=""presentation"" style=""width:100%;max-width:600px;border:none;border-spacing:0;font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif;"" cellpadding=""0"" cellspacing=""0"">

          <!-- ══ HEADER ══ -->
          <tr>
            <td align=""center"" style=""background:linear-gradient(135deg,#0d9488 0%,#0f766e 50%,#115e59 100%);padding:50px 28px 70px 28px;border-radius:20px 20px 0 0;"">
              <div style=""width:72px;height:72px;border-radius:50%;background:rgba(255,255,255,0.18);display:inline-block;line-height:72px;font-size:36px;margin-bottom:14px;border:2px solid rgba(255,255,255,0.3);"">👨‍⚕️</div>
              <h1 style=""margin:0;color:#ffffff;font-size:26px;font-weight:800;letter-spacing:-0.5px;"">Welcome, Dr. {name}!</h1>
              <p style=""margin:8px 0 0 0;color:rgba(255,255,255,0.85);font-size:16px;font-weight:400;"">Your practice on Tabibak starts now</p>
            </td>
          </tr>

          <!-- ══ INTRO CARD ══ -->
          <tr>
            <td align=""center"" style=""padding:0 16px;"">
              <table role=""presentation"" style=""width:100%;border:none;border-spacing:0;background-color:#ffffff;border-radius:16px;box-shadow:0 8px 30px rgba(13,148,136,0.12);margin-top:-50px;"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td class=""mobile-pad"" style=""padding:36px 32px 28px 32px;color:#334155;font-size:15px;line-height:1.7;"">
                    <p style=""margin:0 0 16px 0;"">We're thrilled to have you join <strong style=""color:#0f766e;"">Tabibak</strong>! To help you get the most out of your experience, here are our <strong>top recommendations</strong> to set up your practice for success:</p>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- ══ RECOMMENDATIONS ══ -->
          <tr>
            <td align=""center"" style=""padding:20px 16px 0 16px;"">
              <table role=""presentation"" style=""width:100%;border:none;border-spacing:0;"" cellpadding=""0"" cellspacing=""0"">

                <!-- Rec #1: Complete Profile -->
                <tr>
                  <td style=""padding-bottom:14px;"">
                    <table role=""presentation"" style=""width:100%;border:none;border-spacing:0;background:#ffffff;border-radius:14px;box-shadow:0 2px 12px rgba(0,0,0,0.06);overflow:hidden;"" cellpadding=""0"" cellspacing=""0"">
                      <tr>
                        <td style=""width:6px;background:linear-gradient(180deg,#0d9488,#14b8a6);""></td>
                        <td class=""rec-card"" style=""padding:22px 24px;"">
                          <table role=""presentation"" style=""width:100%;border:none;border-spacing:0;"" cellpadding=""0"" cellspacing=""0"">
                            <tr>
                              <td style=""width:52px;vertical-align:top;"">
                                <div style=""width:44px;height:44px;border-radius:12px;background:linear-gradient(135deg,#f0fdfa,#ccfbf1);text-align:center;line-height:44px;font-size:22px;"">👤</div>
                              </td>
                              <td style=""vertical-align:top;padding-left:14px;"">
                                <table role=""presentation"" style=""border:none;border-spacing:0;width:100%;"" cellpadding=""0"" cellspacing=""0"">
                                  <tr><td>
                                    <span style=""display:inline-block;background:#0d9488;color:#fff;font-size:10px;font-weight:700;padding:2px 8px;border-radius:10px;letter-spacing:0.5px;margin-bottom:6px;"">STEP 1</span>
                                    <h3 style=""margin:6px 0 4px 0;color:#0f172a;font-size:16px;font-weight:700;"">Complete Your Profile</h3>
                                    <p style=""margin:0;color:#64748b;font-size:13px;line-height:1.5;"">Add your <strong>specialization</strong>, <strong>profile photo</strong>, and <strong>ID verification</strong>. A complete profile builds trust — doctors with photos get <strong style=""color:#0d9488;"">3× more bookings</strong>.</p>
                                  </td></tr>
                                </table>
                              </td>
                            </tr>
                          </table>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>

                <!-- Rec #2: Set Schedule -->
                <tr>
                  <td style=""padding-bottom:14px;"">
                    <table role=""presentation"" style=""width:100%;border:none;border-spacing:0;background:#ffffff;border-radius:14px;box-shadow:0 2px 12px rgba(0,0,0,0.06);overflow:hidden;"" cellpadding=""0"" cellspacing=""0"">
                      <tr>
                        <td style=""width:6px;background:linear-gradient(180deg,#14b8a6,#2dd4bf);""></td>
                        <td class=""rec-card"" style=""padding:22px 24px;"">
                          <table role=""presentation"" style=""width:100%;border:none;border-spacing:0;"" cellpadding=""0"" cellspacing=""0"">
                            <tr>
                              <td style=""width:52px;vertical-align:top;"">
                                <div style=""width:44px;height:44px;border-radius:12px;background:linear-gradient(135deg,#f0fdfa,#ccfbf1);text-align:center;line-height:44px;font-size:22px;"">📅</div>
                              </td>
                              <td style=""vertical-align:top;padding-left:14px;"">
                                <table role=""presentation"" style=""border:none;border-spacing:0;width:100%;"" cellpadding=""0"" cellspacing=""0"">
                                  <tr><td>
                                    <span style=""display:inline-block;background:#14b8a6;color:#fff;font-size:10px;font-weight:700;padding:2px 8px;border-radius:10px;letter-spacing:0.5px;margin-bottom:6px;"">STEP 2</span>
                                    <h3 style=""margin:6px 0 4px 0;color:#0f172a;font-size:16px;font-weight:700;"">Define Your Working Schedule</h3>
                                    <p style=""margin:0;color:#64748b;font-size:13px;line-height:1.5;"">Set the <strong>days</strong> and <strong>hours</strong> you're available at the clinic. Patients can only book within your defined schedule — no surprise appointments!</p>
                                  </td></tr>
                                </table>
                              </td>
                            </tr>
                          </table>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>

                <!-- Rec #3: Set Price -->
                <tr>
                  <td style=""padding-bottom:14px;"">
                    <table role=""presentation"" style=""width:100%;border:none;border-spacing:0;background:#ffffff;border-radius:14px;box-shadow:0 2px 12px rgba(0,0,0,0.06);overflow:hidden;"" cellpadding=""0"" cellspacing=""0"">
                      <tr>
                        <td style=""width:6px;background:linear-gradient(180deg,#2dd4bf,#5eead4);""></td>
                        <td class=""rec-card"" style=""padding:22px 24px;"">
                          <table role=""presentation"" style=""width:100%;border:none;border-spacing:0;"" cellpadding=""0"" cellspacing=""0"">
                            <tr>
                              <td style=""width:52px;vertical-align:top;"">
                                <div style=""width:44px;height:44px;border-radius:12px;background:linear-gradient(135deg,#f0fdfa,#ccfbf1);text-align:center;line-height:44px;font-size:22px;"">💰</div>
                              </td>
                              <td style=""vertical-align:top;padding-left:14px;"">
                                <table role=""presentation"" style=""border:none;border-spacing:0;width:100%;"" cellpadding=""0"" cellspacing=""0"">
                                  <tr><td>
                                    <span style=""display:inline-block;background:#2dd4bf;color:#0f766e;font-size:10px;font-weight:700;padding:2px 8px;border-radius:10px;letter-spacing:0.5px;margin-bottom:6px;"">STEP 3</span>
                                    <h3 style=""margin:6px 0 4px 0;color:#0f172a;font-size:16px;font-weight:700;"">Set Your Consultation Price</h3>
                                    <p style=""margin:0;color:#64748b;font-size:13px;line-height:1.5;"">Define your <strong>appointment fee</strong> so patients know upfront. Transparent pricing leads to <strong style=""color:#0d9488;"">higher patient satisfaction</strong> and fewer cancellations.</p>
                                  </td></tr>
                                </table>
                              </td>
                            </tr>
                          </table>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>

                <!-- Rec #4: Community -->
                <tr>
                  <td style=""padding-bottom:14px;"">
                    <table role=""presentation"" style=""width:100%;border:none;border-spacing:0;background:#ffffff;border-radius:14px;box-shadow:0 2px 12px rgba(0,0,0,0.06);overflow:hidden;"" cellpadding=""0"" cellspacing=""0"">
                      <tr>
                        <td style=""width:6px;background:linear-gradient(180deg,#5eead4,#99f6e4);""></td>
                        <td class=""rec-card"" style=""padding:22px 24px;"">
                          <table role=""presentation"" style=""width:100%;border:none;border-spacing:0;"" cellpadding=""0"" cellspacing=""0"">
                            <tr>
                              <td style=""width:52px;vertical-align:top;"">
                                <div style=""width:44px;height:44px;border-radius:12px;background:linear-gradient(135deg,#f0fdfa,#ccfbf1);text-align:center;line-height:44px;font-size:22px;"">💬</div>
                              </td>
                              <td style=""vertical-align:top;padding-left:14px;"">
                                <table role=""presentation"" style=""border:none;border-spacing:0;width:100%;"" cellpadding=""0"" cellspacing=""0"">
                                  <tr><td>
                                    <span style=""display:inline-block;background:#99f6e4;color:#0f766e;font-size:10px;font-weight:700;padding:2px 8px;border-radius:10px;letter-spacing:0.5px;margin-bottom:6px;"">STEP 4</span>
                                    <h3 style=""margin:6px 0 4px 0;color:#0f172a;font-size:16px;font-weight:700;"">Share Health Tips in Community</h3>
                                    <p style=""margin:0;color:#64748b;font-size:13px;line-height:1.5;"">Post <strong>health advice</strong> in the community feed. Active doctors who share tips regularly gain <strong style=""color:#0d9488;"">more visibility</strong> and build a loyal patient base.</p>
                                  </td></tr>
                                </table>
                              </td>
                            </tr>
                          </table>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>

                <!-- Rec #5: Track Finances -->
                <tr>
                  <td style=""padding-bottom:14px;"">
                    <table role=""presentation"" style=""width:100%;border:none;border-spacing:0;background:#ffffff;border-radius:14px;box-shadow:0 2px 12px rgba(0,0,0,0.06);overflow:hidden;"" cellpadding=""0"" cellspacing=""0"">
                      <tr>
                        <td style=""width:6px;background:linear-gradient(180deg,#0d9488,#14b8a6);""></td>
                        <td class=""rec-card"" style=""padding:22px 24px;"">
                          <table role=""presentation"" style=""width:100%;border:none;border-spacing:0;"" cellpadding=""0"" cellspacing=""0"">
                            <tr>
                              <td style=""width:52px;vertical-align:top;"">
                                <div style=""width:44px;height:44px;border-radius:12px;background:linear-gradient(135deg,#f0fdfa,#ccfbf1);text-align:center;line-height:44px;font-size:22px;"">📊</div>
                              </td>
                              <td style=""vertical-align:top;padding-left:14px;"">
                                <table role=""presentation"" style=""border:none;border-spacing:0;width:100%;"" cellpadding=""0"" cellspacing=""0"">
                                  <tr><td>
                                    <span style=""display:inline-block;background:#0d9488;color:#fff;font-size:10px;font-weight:700;padding:2px 8px;border-radius:10px;letter-spacing:0.5px;margin-bottom:6px;"">STEP 5</span>
                                    <h3 style=""margin:6px 0 4px 0;color:#0f172a;font-size:16px;font-weight:700;"">Track Your Finances</h3>
                                    <p style=""margin:0;color:#64748b;font-size:13px;line-height:1.5;"">Use the <strong>Financial Dashboard</strong> to see daily earnings, total revenue, and appointment trends. Know exactly how your practice is performing!</p>
                                  </td></tr>
                                </table>
                              </td>
                            </tr>
                          </table>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>

                <!-- Rec #6: Stay Available -->
                <tr>
                  <td style=""padding-bottom:14px;"">
                    <table role=""presentation"" style=""width:100%;border:none;border-spacing:0;background:#ffffff;border-radius:14px;box-shadow:0 2px 12px rgba(0,0,0,0.06);overflow:hidden;"" cellpadding=""0"" cellspacing=""0"">
                      <tr>
                        <td style=""width:6px;background:linear-gradient(180deg,#14b8a6,#0d9488);""></td>
                        <td class=""rec-card"" style=""padding:22px 24px;"">
                          <table role=""presentation"" style=""width:100%;border:none;border-spacing:0;"" cellpadding=""0"" cellspacing=""0"">
                            <tr>
                              <td style=""width:52px;vertical-align:top;"">
                                <div style=""width:44px;height:44px;border-radius:12px;background:linear-gradient(135deg,#f0fdfa,#ccfbf1);text-align:center;line-height:44px;font-size:22px;"">✅</div>
                              </td>
                              <td style=""vertical-align:top;padding-left:14px;"">
                                <table role=""presentation"" style=""border:none;border-spacing:0;width:100%;"" cellpadding=""0"" cellspacing=""0"">
                                  <tr><td>
                                    <span style=""display:inline-block;background:#14b8a6;color:#fff;font-size:10px;font-weight:700;padding:2px 8px;border-radius:10px;letter-spacing:0.5px;margin-bottom:6px;"">STEP 6</span>
                                    <h3 style=""margin:6px 0 4px 0;color:#0f172a;font-size:16px;font-weight:700;"">Toggle Your Availability</h3>
                                    <p style=""margin:0;color:#64748b;font-size:13px;line-height:1.5;"">Going on leave? Use the <strong>availability toggle</strong> to pause bookings without losing your schedule. Turn it back on when you're ready!</p>
                                  </td></tr>
                                </table>
                              </td>
                            </tr>
                          </table>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>

              </table>
            </td>
          </tr>

          <!-- ══ CTA BUTTON ══ -->
          <tr>
            <td align=""center"" style=""padding:28px 20px 0 20px;"">
              <table role=""presentation"" style=""border:none;border-spacing:0;"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td align=""center"" style=""background:linear-gradient(135deg,#0d9488,#0f766e);border-radius:30px;box-shadow:0 4px 14px rgba(13,148,136,0.35);"">
                    <a href=""#"" style=""display:inline-block;padding:16px 48px;color:#ffffff;font-size:16px;font-weight:700;text-decoration:none;border-radius:30px;letter-spacing:0.3px;"">Start Setting Up →</a>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- ══ PRO TIP ══ -->
          <tr>
            <td align=""center"" style=""padding:28px 16px 0 16px;"">
              <table role=""presentation"" style=""width:100%;border:none;border-spacing:0;background:#ffffff;border-radius:14px;box-shadow:0 2px 12px rgba(0,0,0,0.06);"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td style=""padding:24px 28px;"">
                    <table role=""presentation"" style=""width:100%;border:none;border-spacing:0;"" cellpadding=""0"" cellspacing=""0"">
                      <tr>
                        <td style=""width:36px;vertical-align:top;font-size:20px;"">💡</td>
                        <td style=""vertical-align:top;padding-left:8px;"">
                          <p style=""margin:0;font-size:14px;font-weight:700;color:#0f766e;"">Pro Tip</p>
                          <p style=""margin:4px 0 0 0;font-size:13px;color:#64748b;line-height:1.5;"">Doctors who complete all 6 steps within the first week see <strong style=""color:#0d9488;"">5× more patient bookings</strong> in their first month. Don't wait — start now!</p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- ══ FOOTER ══ -->
          <tr>
            <td align=""center"" style=""padding:36px 20px 20px 20px;"">
              <h2 style=""margin:0;color:#0f766e;font-size:18px;font-weight:700;"">Tabibak</h2>
              <p style=""margin:4px 0 0 0;color:#64748b;font-size:13px;"">Your practice, elevated.</p>
              {RenderSocialLinks()}
              <p style=""margin:16px 0 0 0;color:#94a3b8;font-size:11px;"">You received this email because you registered as a doctor on Tabibak.</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</div>
</body>
</html>";
    }

    public static string ForgotPasswordCode(string code, string expiryMessage = "5 minutes")
    {
        return $@"<!DOCTYPE html><html><head><meta charset=""utf-8""/><meta name=""viewport"" content=""width=device-width,initial-scale=1""/></head>
<body style=""margin:0;padding:0;font-family:'Segoe UI',-apple-system,BlinkMacSystemFont,Roboto,Helvetica,Arial,sans-serif;background:#f0fdf9;-webkit-font-smoothing:antialiased"">
<div style=""padding:32px 16px"">
<table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0""><tr><td align=""center"">
<table width=""560"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""max-width:560px;background:#ffffff;border-radius:20px;overflow:hidden;box-shadow:0 8px 40px rgba(13,148,136,0.10)"">
  <tr><td style=""background:linear-gradient(135deg,#0d9488 0%,#0f766e 50%,#115e59 100%);padding:44px 36px 48px;text-align:center"">
    <div style=""width:56px;height:56px;border-radius:50%;background:rgba(255,255,255,0.20);display:inline-block;line-height:56px;font-size:26px;margin-bottom:16px"">🔐</div>
    <h1 style=""margin:0;color:#ffffff;font-size:22px;font-weight:700;letter-spacing:-0.3px"">Password Reset Code</h1>
    <p style=""margin:6px 0 0;color:rgba(255,255,255,0.80);font-size:14px;font-weight:400"">We received a request to reset your password</p>
  </td></tr>
  <tr><td style=""padding:36px 36px 28px;color:#334155;font-size:15px;line-height:1.7"">
    <p style=""margin:0 0 16px"">Use the code below to reset your password. <strong>Do not share it with anyone.</strong></p>
    {RenderCodeDigits(code)}
    <div style=""background:#f0fdfa;border-left:4px solid #0d9488;border-radius:0 12px 12px 0;padding:18px 22px;margin:20px 0;color:#0f766e;font-weight:600;font-size:15px"">⏱ This code expires in <strong>{System.Net.WebUtility.HtmlEncode(expiryMessage)}</strong></div>
    <p style=""margin:0;color:#94a3b8;font-size:13px"">If you didn't request a password reset, you can safely ignore this email — your account is secure.</p>
  </td></tr>
  <tr><td style=""padding:24px 36px;text-align:center;background:#f8fffe;border-top:1px solid #e5e7eb"">
    <p style=""margin:0;color:#0d9488;font-weight:700;font-size:13px"">Tabibak</p>
    <p style=""margin:4px 0 0;color:#94a3b8;font-size:12px"">Your health, our priority</p>
    {RenderSocialLinks()}
  </td></tr>
</table>
</td></tr></table>
</div>
</body></html>";
    }

    public static string ChangePasswordCode(string code, string expiryMessage = "5 minutes")
    {
        return $@"<!DOCTYPE html><html><head><meta charset=""utf-8""/><meta name=""viewport"" content=""width=device-width,initial-scale=1""/></head>
<body style=""margin:0;padding:0;font-family:'Segoe UI',-apple-system,BlinkMacSystemFont,Roboto,Helvetica,Arial,sans-serif;background:#f0fdf9;-webkit-font-smoothing:antialiased"">
<div style=""padding:32px 16px"">
<table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0""><tr><td align=""center"">
<table width=""560"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""max-width:560px;background:#ffffff;border-radius:20px;overflow:hidden;box-shadow:0 8px 40px rgba(13,148,136,0.10)"">
  <tr><td style=""background:linear-gradient(135deg,#0d9488 0%,#0f766e 50%,#115e59 100%);padding:44px 36px 48px;text-align:center"">
    <div style=""width:56px;height:56px;border-radius:50%;background:rgba(255,255,255,0.20);display:inline-block;line-height:56px;font-size:26px;margin-bottom:16px"">🔑</div>
    <h1 style=""margin:0;color:#ffffff;font-size:22px;font-weight:700;letter-spacing:-0.3px"">Change Password Code</h1>
    <p style=""margin:6px 0 0;color:rgba(255,255,255,0.80);font-size:14px;font-weight:400"">Verify your identity to update your password</p>
  </td></tr>
  <tr><td style=""padding:36px 36px 28px;color:#334155;font-size:15px;line-height:1.7"">
    <p style=""margin:0 0 16px"">Use the code below to confirm your password change. <strong>Do not share it with anyone.</strong></p>
    {RenderCodeDigits(code)}
    <div style=""background:#f0fdfa;border-left:4px solid #0d9488;border-radius:0 12px 12px 0;padding:18px 22px;margin:20px 0;color:#0f766e;font-weight:600;font-size:15px"">⏱ This code expires in <strong>{System.Net.WebUtility.HtmlEncode(expiryMessage)}</strong></div>
    <p style=""margin:0;color:#94a3b8;font-size:13px"">If you didn't initiate this change, secure your account immediately by resetting your password.</p>
  </td></tr>
  <tr><td style=""padding:24px 36px;text-align:center;background:#f8fffe;border-top:1px solid #e5e7eb"">
    <p style=""margin:0;color:#0d9488;font-weight:700;font-size:13px"">Tabibak</p>
    <p style=""margin:4px 0 0;color:#94a3b8;font-size:12px"">Your health, our priority</p>
    {RenderSocialLinks()}
  </td></tr>
</table>
</td></tr></table>
</div>
</body></html>";
    }

    public static string PasswordChanged()
    {
        return $@"<!DOCTYPE html><html><head><meta charset=""utf-8""/><meta name=""viewport"" content=""width=device-width,initial-scale=1""/></head>
<body style=""margin:0;padding:0;font-family:'Segoe UI',-apple-system,BlinkMacSystemFont,Roboto,Helvetica,Arial,sans-serif;background:#f0fdf9;-webkit-font-smoothing:antialiased"">
<div style=""padding:32px 16px"">
<table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0""><tr><td align=""center"">
<table width=""560"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""max-width:560px;background:#ffffff;border-radius:20px;overflow:hidden;box-shadow:0 8px 40px rgba(13,148,136,0.10)"">
  <tr><td style=""background:linear-gradient(135deg,#0d9488 0%,#0f766e 50%,#115e59 100%);padding:44px 36px 48px;text-align:center"">
    <div style=""width:56px;height:56px;border-radius:50%;background:rgba(255,255,255,0.20);display:inline-block;line-height:56px;font-size:26px;margin-bottom:16px"">✅</div>
    <h1 style=""margin:0;color:#ffffff;font-size:22px;font-weight:700;letter-spacing:-0.3px"">Password Updated</h1>
    <p style=""margin:6px 0 0;color:rgba(255,255,255,0.80);font-size:14px;font-weight:400"">Your account security has been updated</p>
  </td></tr>
  <tr><td style=""padding:36px 36px 28px;color:#334155;font-size:15px;line-height:1.7"">
    <p style=""margin:0 0 16px"">Your password was changed successfully. You can now sign in with your new password.</p>
    <div style=""border-top:1px solid #e5e7eb;margin:24px 0""></div>
    <p style=""margin:0;color:#94a3b8;font-size:13px"">⚠️ If you didn't make this change, please contact our support team immediately to secure your account.</p>
  </td></tr>
  <tr><td style=""padding:24px 36px;text-align:center;background:#f8fffe;border-top:1px solid #e5e7eb"">
    <p style=""margin:0;color:#0d9488;font-weight:700;font-size:13px"">Tabibak</p>
    <p style=""margin:4px 0 0;color:#94a3b8;font-size:12px"">Your health, our priority</p>
    {RenderSocialLinks()}
  </td></tr>
</table>
</td></tr></table>
</div>
</body></html>";
    }

    public static string AppointmentSuccess(string patientName, string doctorName, DateTime appointmentDate)
    {
        var p = System.Net.WebUtility.HtmlEncode(patientName ?? "Patient");
        var d = System.Net.WebUtility.HtmlEncode(doctorName ?? "Doctor");
        var dateStr = System.Net.WebUtility.HtmlEncode(
            appointmentDate.ToString("dddd, MMMM d, yyyy 'at' h:mm tt"));

        return $@"<!DOCTYPE html><html><head><meta charset=""utf-8""/><meta name=""viewport"" content=""width=device-width,initial-scale=1""/></head>
<body style=""margin:0;padding:0;font-family:'Segoe UI',-apple-system,BlinkMacSystemFont,Roboto,Helvetica,Arial,sans-serif;background:#f0fdf9;-webkit-font-smoothing:antialiased"">
<div style=""padding:32px 16px"">
<table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0""><tr><td align=""center"">
<table width=""560"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""max-width:560px;background:#ffffff;border-radius:20px;overflow:hidden;box-shadow:0 8px 40px rgba(13,148,136,0.10)"">
  <tr><td style=""background:linear-gradient(135deg,#0d9488 0%,#0f766e 50%,#115e59 100%);padding:44px 36px 48px;text-align:center"">
    <div style=""width:56px;height:56px;border-radius:50%;background:rgba(255,255,255,0.20);display:inline-block;line-height:56px;font-size:26px;margin-bottom:16px"">📅</div>
    <h1 style=""margin:0;color:#ffffff;font-size:22px;font-weight:700;letter-spacing:-0.3px"">Appointment Confirmed</h1>
    <p style=""margin:6px 0 0;color:rgba(255,255,255,0.80);font-size:14px;font-weight:400"">Your booking has been processed successfully</p>
  </td></tr>
  <tr><td style=""padding:36px 36px 28px;color:#334155;font-size:15px;line-height:1.7"">
    <p style=""margin:0 0 16px"">Great news! Your appointment has been confirmed.</p>
    
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin:20px 0"">
      <tr><td style=""padding:12px 16px;border-bottom:1px solid #e2e8f0;font-weight:600;color:#0f766e;width:42%"">👤 Patient</td><td style=""padding:12px 16px;border-bottom:1px solid #e2e8f0;color:#334155"">{p}</td></tr>
      <tr><td style=""padding:12px 16px;border-bottom:1px solid #e2e8f0;font-weight:600;color:#0f766e;width:42%"">🩺 Doctor</td><td style=""padding:12px 16px;border-bottom:1px solid #e2e8f0;color:#334155"">{d}</td></tr>
      <tr><td style=""padding:12px 16px;font-weight:600;color:#0f766e;width:42%"">📅 Date &amp; Time</td><td style=""padding:12px 16px;color:#334155"">{dateStr}</td></tr>
    </table>

    <p style=""margin:0 0 16px"">You can check your account for the latest status or to make changes.</p>
    <p style=""margin:0"">Thank you for choosing Tabibak! 💚</p>
  </td></tr>
  <tr><td style=""padding:24px 36px;text-align:center;background:#f8fffe;border-top:1px solid #e5e7eb"">
    <p style=""margin:0;color:#0d9488;font-weight:700;font-size:13px"">Tabibak</p>
    <p style=""margin:4px 0 0;color:#94a3b8;font-size:12px"">Your health, our priority</p>
    {RenderSocialLinks()}
  </td></tr>
</table>
</td></tr></table>
</div>
</body></html>";
    }

    public static string AppointmentReminder(string patientName, string doctorName, DateTime appointmentDateTime, int minutesUntil)
    {
        var p = System.Net.WebUtility.HtmlEncode(patientName ?? "Patient");
        var d = System.Net.WebUtility.HtmlEncode(doctorName ?? "Doctor");
        var dateStr = System.Net.WebUtility.HtmlEncode(
            appointmentDateTime.ToString("dddd, MMMM d, yyyy 'at' h:mm tt"));
        var timeLeft = minutesUntil <= 60
            ? $"{minutesUntil} minute{(minutesUntil == 1 ? "" : "s")}"
            : $"{minutesUntil / 60} hour{((minutesUntil / 60) == 1 ? "" : "s")}";

        return $@"<!DOCTYPE html><html><head><meta charset=""utf-8""/><meta name=""viewport"" content=""width=device-width,initial-scale=1""/></head>
<body style=""margin:0;padding:0;font-family:'Segoe UI',-apple-system,BlinkMacSystemFont,Roboto,Helvetica,Arial,sans-serif;background:#f0fdf9;-webkit-font-smoothing:antialiased"">
<div style=""padding:32px 16px"">
<table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0""><tr><td align=""center"">
<table width=""560"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""max-width:560px;background:#ffffff;border-radius:20px;overflow:hidden;box-shadow:0 8px 40px rgba(13,148,136,0.10)"">
  <tr><td style=""background:linear-gradient(135deg,#0d9488 0%,#0f766e 50%,#115e59 100%);padding:44px 36px 48px;text-align:center"">
    <div style=""width:56px;height:56px;border-radius:50%;background:rgba(255,255,255,0.20);display:inline-block;line-height:56px;font-size:26px;margin-bottom:16px"">⏰</div>
    <h1 style=""margin:0;color:#ffffff;font-size:22px;font-weight:700;letter-spacing:-0.3px"">Appointment Reminder</h1>
    <p style=""margin:6px 0 0;color:rgba(255,255,255,0.80);font-size:14px;font-weight:400"">Your visit is coming up soon</p>
  </td></tr>
  <tr><td style=""padding:36px 36px 28px;color:#334155;font-size:15px;line-height:1.7"">
    <p style=""margin:0 0 16px"">Hello <strong>{p}</strong>,</p>
    <p style=""margin:0 0 16px"">This is a friendly reminder that your appointment is in <strong>{timeLeft}</strong>.</p>
    
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin:20px 0"">
      <tr><td style=""padding:12px 16px;border-bottom:1px solid #e2e8f0;font-weight:600;color:#0f766e;width:42%"">🩺 Doctor</td><td style=""padding:12px 16px;border-bottom:1px solid #e2e8f0;color:#334155"">{d}</td></tr>
      <tr><td style=""padding:12px 16px;font-weight:600;color:#0f766e;width:42%"">📅 Date &amp; Time</td><td style=""padding:12px 16px;color:#334155"">{dateStr}</td></tr>
    </table>

    <div style=""background:#f0fdfa;border-left:4px solid #0d9488;border-radius:0 12px 12px 0;padding:18px 22px;margin:20px 0;color:#0f766e;font-weight:600;font-size:15px"">⏱ <strong>{timeLeft}</strong> until your appointment</div>
    
    <p style=""margin:0 0 16px"">Please arrive on time. If you need to reschedule or cancel, do so from your Tabibak account.</p>
    <p style=""margin:0"">We look forward to seeing you! 😊</p>
  </td></tr>
  <tr><td style=""padding:24px 36px;text-align:center;background:#f8fffe;border-top:1px solid #e5e7eb"">
    <p style=""margin:0;color:#0d9488;font-weight:700;font-size:13px"">Tabibak</p>
    <p style=""margin:4px 0 0;color:#94a3b8;font-size:12px"">Your health, our priority</p>
    {RenderSocialLinks()}
  </td></tr>
</table>
</td></tr></table>
</div>
</body></html>";
    }

    public static string AppointmentSuccessReschedule(string patientName, string doctorName, DateTime newDate)
    {
        var p = System.Net.WebUtility.HtmlEncode(patientName ?? "Patient");
        var d = System.Net.WebUtility.HtmlEncode(doctorName ?? "Doctor");
        var dateStr = System.Net.WebUtility.HtmlEncode(
            newDate.ToString("dddd, MMMM d, yyyy 'at' h:mm tt"));

        return $@"<!DOCTYPE html><html><head><meta charset=""utf-8""/><meta name=""viewport"" content=""width=device-width,initial-scale=1""/></head>
<body style=""margin:0;padding:0;font-family:'Segoe UI',-apple-system,BlinkMacSystemFont,Roboto,Helvetica,Arial,sans-serif;background:#f0fdf9;-webkit-font-smoothing:antialiased"">
<div style=""padding:32px 16px"">
<table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0""><tr><td align=""center"">
<table width=""560"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""max-width:560px;background:#ffffff;border-radius:20px;overflow:hidden;box-shadow:0 8px 40px rgba(13,148,136,0.10)"">
  <tr><td style=""background:linear-gradient(135deg,#0d9488 0%,#0f766e 50%,#115e59 100%);padding:44px 36px 48px;text-align:center"">
    <div style=""width:56px;height:56px;border-radius:50%;background:rgba(255,255,255,0.20);display:inline-block;line-height:56px;font-size:26px;margin-bottom:16px"">🔄</div>
    <h1 style=""margin:0;color:#ffffff;font-size:22px;font-weight:700;letter-spacing:-0.3px"">Appointment Rescheduled</h1>
    <p style=""margin:6px 0 0;color:rgba(255,255,255,0.80);font-size:14px;font-weight:400"">Your booking has been updated to a new time</p>
  </td></tr>
  <tr><td style=""padding:36px 36px 28px;color:#334155;font-size:15px;line-height:1.7"">
    <p style=""margin:0 0 16px"">Your appointment has been rescheduled successfully.</p>
    
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""margin:20px 0"">
      <tr><td style=""padding:12px 16px;border-bottom:1px solid #e2e8f0;font-weight:600;color:#0f766e;width:42%"">👤 Patient</td><td style=""padding:12px 16px;border-bottom:1px solid #e2e8f0;color:#334155"">{p}</td></tr>
      <tr><td style=""padding:12px 16px;border-bottom:1px solid #e2e8f0;font-weight:600;color:#0f766e;width:42%"">🩺 Doctor</td><td style=""padding:12px 16px;border-bottom:1px solid #e2e8f0;color:#334155"">{d}</td></tr>
      <tr><td style=""padding:12px 16px;font-weight:600;color:#0f766e;width:42%"">📅 New Date &amp; Time</td><td style=""padding:12px 16px;color:#334155"">{dateStr}</td></tr>
    </table>

    <div style=""background:#f0fdfa;border-left:4px solid #0d9488;border-radius:0 12px 12px 0;padding:18px 22px;margin:20px 0;color:#0f766e;font-weight:600;font-size:15px"">📌 Please update your calendar with the new date above</div>
    <p style=""margin:0"">Thank you for using Tabibak! 💚</p>
  </td></tr>
  <tr><td style=""padding:24px 36px;text-align:center;background:#f8fffe;border-top:1px solid #e5e7eb"">
    <p style=""margin:0;color:#0d9488;font-weight:700;font-size:13px"">Tabibak</p>
    <p style=""margin:4px 0 0;color:#94a3b8;font-size:12px"">Your health, our priority</p>
    {RenderSocialLinks()}
  </td></tr>
</table>
</td></tr></table>
</div>
</body></html>";
    }

    public static string EmailVerificationCode(string code, string expiryMessage = "60 seconds")
    {
        return $@"<!DOCTYPE html><html><head><meta charset=""utf-8""/><meta name=""viewport"" content=""width=device-width,initial-scale=1""/></head>
<body style=""margin:0;padding:0;font-family:'Segoe UI',-apple-system,BlinkMacSystemFont,Roboto,Helvetica,Arial,sans-serif;background:#f0fdf9;-webkit-font-smoothing:antialiased"">
<div style=""padding:32px 16px"">
<table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0""><tr><td align=""center"">
<table width=""560"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""max-width:560px;background:#ffffff;border-radius:20px;overflow:hidden;box-shadow:0 8px 40px rgba(13,148,136,0.10),0 1px 3px rgba(0,0,0,0.04)"">
  <!-- Header -->
  <tr><td style=""background:linear-gradient(135deg,#0d9488 0%,#0f766e 50%,#115e59 100%);padding:44px 36px 48px;text-align:center"">
    <div style=""width:56px;height:56px;border-radius:50%;background:rgba(255,255,255,0.20);display:inline-block;line-height:56px;font-size:26px;margin-bottom:16px"">✉️</div>
    <h1 style=""margin:0;color:#ffffff;font-size:22px;font-weight:700;letter-spacing:-0.3px"">Verify Your Email</h1>
    <p style=""margin:6px 0 0;color:rgba(255,255,255,0.80);font-size:14px;font-weight:400"">One last step to activate your account</p>
  </td></tr>
  <!-- Body -->
  <tr><td style=""padding:36px 36px 28px;color:#334155;font-size:15px;line-height:1.7"">
    <p style=""margin:0 0 16px"">Welcome to <strong>Tabibak</strong>! Enter the code below to verify your email address and complete your registration.</p>
    {RenderCodeDigits(code)}
    <div style=""background:#f0fdfa;border-left:4px solid #0d9488;border-radius:0 12px 12px 0;padding:18px 22px;margin:20px 0;color:#0f766e;font-weight:600;font-size:15px"">⚡ This code expires in <strong>{System.Net.WebUtility.HtmlEncode(expiryMessage)}</strong> — act fast!</div>
    <p style=""margin:0;color:#94a3b8;font-size:13px"">If you didn't create an account on Tabibak, you can safely ignore this email.</p>
  </td></tr>
  <!-- Footer -->
  <tr><td style=""padding:24px 36px;text-align:center;background:#f8fffe;border-top:1px solid #e5e7eb"">
    <p style=""margin:0;color:#0d9488;font-weight:700;font-size:13px"">Tabibak</p>
    <p style=""margin:4px 0 0;color:#94a3b8;font-size:12px"">Your health, our priority</p>
    {RenderSocialLinks()}
  </td></tr>
</table>
</td></tr></table>
</div>
</body></html>";
    }
}
