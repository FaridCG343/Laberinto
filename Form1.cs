using LaberintoChido.Properties;

namespace LaberintoChido
{
    public partial class frmcgfLaberinto : Form
    {
        readonly List<PictureBox> walls;
        readonly List<PictureBox> enemies;
        bool portalEnCooldown = false, llaveObtenida = false;
        Thread movEnemigo1, movEnemigo2;
        CancellationTokenSource enemigo1TokenSource, enemigo2TokenSource;
        public frmcgfLaberinto()
        {
            DoubleBuffered = true;
            InitializeComponent();
            walls = new()
            {
                pbcgfWall1, pbcgfWall2, pbcgfWall3, pbcgfWall4,
                pbcgfWall5, pbcgfWall6, pbcgfWall7, pbcgfWall8,
                pbcgfWall9, pbcgfWall10, pbcgfWall11, pbcgfWall12,
                pbcgfWall13, pbcgfWall14, pbcgfWall15, pbcgfWall16,
                pbcgfWall17
            };
            enemies = new()
            {
                pbcgfEnemy2, pbcgfEnemy1
            };
            pbcgfEnemy2.BringToFront();
            pbcgfEnemy1.BringToFront();
            pbcgfBruja.BringToFront();
            enemigo1TokenSource = new();
            enemigo2TokenSource = new();
            movEnemigo1 = new(() => moverEnemigo(enemigo1TokenSource.Token, pbcgfEnemy1, 90, 320));
            movEnemigo2 = new(() => moverEnemigo(enemigo2TokenSource.Token, pbcgfEnemy2, 485, 865));
        }

        private void moverEnemigo(CancellationToken token, PictureBox enemy, int lim1, int lim2)
        {
            int direccion = -1;
            Point nuevoPunto = enemy.Location;
            do
            {
                nuevoPunto.X += direccion;
                enemy.Location = nuevoPunto;
                if (nuevoPunto.X <= lim1 || nuevoPunto.X >= lim2)
                {
                    enemy.Image = Resources.SkeletonIdle;
                    Thread.Sleep(5000);
                    direccion *= -1;
                    enemy.Image = direccion == -1 ? Resources.SkeletonWalkBack : Resources.SkeletonWalk;
                }
                else
                {
                    Thread.Sleep(15);
                }
            } while (!token.IsCancellationRequested);
        }

        private void frmcgfLaberinto_KeyDown(object sender, KeyEventArgs e)
        {
            int movimiento = 2; // Ajusta la cantidad de movimiento según sea necesario
            Point nuevaPosicion = pbcgfBruja.Location;
            Point original = pbcgfBruja.Location;
            switch (e.KeyCode)
            {
                case Keys.Up:
                    if ((string)pbcgfBruja.Tag == "idle")
                    {
                        pbcgfBruja.Tag = "walk";
                        pbcgfBruja.Image = Resources.W_witch_run;
                    }
                    nuevaPosicion.Y -= movimiento;
                    break;
                case Keys.Down:
                    if ((string)pbcgfBruja.Tag == "idle")
                    {
                        pbcgfBruja.Tag = "walk";
                        pbcgfBruja.Image = Resources.W_witch_run;
                    }
                    nuevaPosicion.Y += movimiento;
                    break;
                case Keys.Left:
                    if ((string)pbcgfBruja.Tag == "idle" || (string)pbcgfBruja.Tag == "walk")
                    {
                        pbcgfBruja.Tag = "walkB";
                        pbcgfBruja.Image = Resources.W_witch_run_back;
                    }
                    nuevaPosicion.X -= movimiento;
                    break;
                case Keys.Right:
                    if ((string)pbcgfBruja.Tag == "idle" || (string)pbcgfBruja.Tag == "walkB")
                    {
                        pbcgfBruja.Tag = "walk";
                        pbcgfBruja.Image = Resources.W_witch_run;
                    }
                    nuevaPosicion.X += movimiento;
                    break;
            }
            pbcgfBruja.Location = nuevaPosicion;

            // Verificar si la nueva posición está dentro de los límites de la ventana
            if (nuevaPosicion.X < 0 || nuevaPosicion.Y < 0 || nuevaPosicion.X + pbcgfBruja.Width > this.ClientSize.Width || nuevaPosicion.Y + pbcgfBruja.Height > this.ClientSize.Height)
            {
                // Si el personaje está fuera de los límites, revertir el movimiento
                pbcgfBruja.Location = original;
            }
            // Verificar si hay colisión con los muros
            else if (ColisionConMuros(pbcgfBruja, walls))
            {
                // Si hay colisión, revertir el movimiento
                pbcgfBruja.Location = original;
            }

            if (ColisionConEnemigos(pbcgfBruja, enemies))
            {
                // Ejecutar la acción correspondiente cuando el personaje colisiona con un enemigo
                PersonajeColisionaConEnemigo();
            }

            TeletransportarSiEntraPortal(pbcgfBruja, pbcgfPortal1, pbcgfPortal2);
            TeletransportarSiEntraPortal(pbcgfBruja, pbcgfPortal3, pbcgfPortal4);

            if (pbcgfKey.Visible && HayColision(pbcgfBruja, pbcgfKey))
            {
                pbcgfKey.Visible = false;
                llaveObtenida = true;
                pbcgfDoor.Image = Resources.unlockedDoor;
            }

            if (llaveObtenida && HayColision(pbcgfBruja, pbcgfDoor))
            {
                MessageBox.Show("Felicidades has logrado escapar del laberinto >:3", "Felicidades");
                reiniciar();
            }
        }

        private void PersonajeColisionaConEnemigo()
        {
            // Reiniciar la posición del personaje, por ejemplo, a la posición inicial
            reiniciar();

            // Mostrar un mensaje indicando que el jugador ha perdido
            MessageBox.Show("¡Has sido atrapado por un enemigo! Inténtalo de nuevo.", "Perdiste", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void reiniciar()
        {
            pbcgfBruja.Location = new(12, 499);
            pbcgfKey.Visible = true;
            llaveObtenida = false;
            pbcgfDoor.Image = Resources.lockedDoor;
        }
        private void TeletransportarSiEntraPortal(PictureBox personaje, PictureBox portal1, PictureBox portal2)
        {
            if (!portalEnCooldown)
            {
                if (HayColision(personaje, portal1))
                {
                    // Teletransportar al personaje a la posición del portal 2
                    personaje.Location = new Point(portal2.Location.X + portal2.Width / 2 - personaje.Width / 2, portal2.Location.Y + portal2.Height / 2 - personaje.Height / 2);
                    IniciarCooldownPortal();
                }
                else if (HayColision(personaje, portal2))
                {
                    // Teletransportar al personaje a la posición del portal 1
                    personaje.Location = new Point(portal1.Location.X + portal1.Width / 2 - personaje.Width / 2, portal1.Location.Y + portal1.Height / 2 - personaje.Height / 2);
                    IniciarCooldownPortal();
                }
            }
        }

        private bool ColisionConEnemigos(PictureBox personaje, List<PictureBox> enemigos)
        {
            foreach (PictureBox enemigo in enemigos)
            {
                if (HayColision(personaje, enemigo))
                {
                    return true;
                }
            }
            return false;
        }


        public void IniciarCooldownPortal()
        {
            portalEnCooldown = true;
            timerPortal.Start();
        }

        private void timerPortal_Tick(object sender, EventArgs e)
        {
            portalEnCooldown = false;
            timerPortal.Stop();
        }

        private bool ColisionConMuros(PictureBox personaje, List<PictureBox> muros)
        {
            foreach (PictureBox muro in muros)
            {
                if (HayColision(personaje, muro))
                {
                    return true;
                }
            }
            return false;
        }



        private bool HayColision(PictureBox personaje, PictureBox objeto)
        {
            Rectangle rectPersonaje = new(personaje.Location, personaje.Size);
            Rectangle rectMuro = new(objeto.Location, objeto.Size);

            return rectPersonaje.IntersectsWith(rectMuro);
        }

        private void frmcgfLaberinto_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Right || e.KeyCode == Keys.Left)
            {
                pbcgfBruja.Tag = "idle";
                pbcgfBruja.Image = Resources.W_witch_idle2;
            }
        }

        private void frmcgfLaberinto_Load(object sender, EventArgs e)
        {
            movEnemigo1.Start();
            movEnemigo2.Start();
        }

        private void frmcgfLaberinto_FormClosed(object sender, FormClosedEventArgs e)
        {
            enemigo1TokenSource.Cancel();
            enemigo2TokenSource.Cancel();
        }
    }
}