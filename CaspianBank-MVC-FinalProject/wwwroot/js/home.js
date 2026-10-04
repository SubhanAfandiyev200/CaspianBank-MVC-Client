(function () {
  const reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

  try {
    const saved = JSON.parse(localStorage.getItem("caspian.ui") || "null");
    const home = saved && saved.home;
    if (home) {
      document.querySelectorAll("[data-ui]").forEach((node) => {
        const value = home[node.getAttribute("data-ui")];
        if (value) node.textContent = value;
      });
    }
  } catch (e) { /* keep the page copy */ }

  const toggle = document.querySelector(".nav-toggle");
  const header = document.querySelector(".site-header");
  toggle?.addEventListener("click", () => {
    const open = header.classList.toggle("is-open");
    header.classList.toggle("is-menu", open);
    toggle.setAttribute("aria-expanded", String(open));
  });

  document.getElementById("footer-form")?.addEventListener("submit", (event) => {
    event.preventDefault();
    const input = document.getElementById("footer-email");
    // TODO: POST { email: input.value } to the mailing-list endpoint.
    input.value = "";
    input.placeholder = "Noted. We will write when there is something to say.";
  });

  const homeNav = document.querySelector(".nav-home");
  const mega = document.getElementById("mega");
  if (homeNav && mega && header) {
    let closeTimer = 0;
    const openMega = () => {
      clearTimeout(closeTimer);
      header.classList.add("is-mega");
    };
    const closeMega = () => {
      clearTimeout(closeTimer);
      closeTimer = setTimeout(() => header.classList.remove("is-mega"), 420);
    };
    homeNav.addEventListener("mouseenter", openMega);
    homeNav.addEventListener("mouseleave", closeMega);
    mega.addEventListener("mouseenter", openMega);
    mega.addEventListener("mouseleave", closeMega);
    homeNav.addEventListener("focusin", openMega);
    mega.addEventListener("focusout", (event) => {
      if (!mega.contains(event.relatedTarget)) header.classList.remove("is-mega");
    });
  }

  const video = document.getElementById("brand-video");
  const videoButton = document.getElementById("video-toggle");
  const videoFrame = document.getElementById("video-frame");
  if (video && videoButton && videoFrame) {
    const source = video.dataset.src;
    if (source) video.src = source;
    document.body.appendChild(videoButton);
    videoButton.hidden = false;
    videoButton.classList.add("video-cursor");
    let previewOn = false;
    const hasFile = () => Boolean(video.getAttribute("src") || video.dataset.src);
    const label = () => {
      const playing = hasFile() ? !video.paused : previewOn;
      videoButton.textContent = playing ? "Stop" : "Play";
    };
    const place = (event) => {
      videoButton.style.left = event.clientX + "px";
      videoButton.style.top = event.clientY + "px";
    };
    label();
    videoFrame.addEventListener("pointermove", (event) => {
      videoFrame.classList.add("is-cursor");
      videoButton.classList.add("is-on");
      place(event);
    });
    videoFrame.addEventListener("pointerleave", () => {
      videoFrame.classList.remove("is-cursor");
      videoButton.classList.remove("is-on");
    });
    videoFrame.addEventListener("click", () => {
      if (hasFile()) {
        if (video.paused) video.play();
        else video.pause();
      } else {
        previewOn = !previewOn;
        label();
      }
    });
    video.addEventListener("play", label);
    video.addEventListener("pause", label);
  }

  if (!window.gsap || !window.ScrollTrigger) return;
  gsap.registerPlugin(ScrollTrigger);
  const hasInertia = typeof window.InertiaPlugin !== "undefined";
  if (window.Draggable && hasInertia) gsap.registerPlugin(Draggable, InertiaPlugin);
  else if (window.Draggable) gsap.registerPlugin(Draggable);

  const fanFrom = [
    { x: 28, y: 34, rotation: 7 },
    { x: 6, y: 12, rotation: 10 },
    { x: -18, y: 40, rotation: 13 }
  ];
  const fanTo = [
    { x: 72, y: 6, rotation: 6 },
    { x: -16, y: -24, rotation: 18 },
    { x: -108, y: 52, rotation: 28 }
  ];
  const cards = gsap.utils.toArray(".fan-card");
  if (cards.length) {
    if (reduce) {
      cards.forEach((card, i) => gsap.set(card, { ...fanTo[i], opacity: 1 }));
    } else {
      cards.forEach((card, i) => gsap.set(card, { ...fanFrom[i], opacity: 1 }));
      gsap.fromTo(cards,
        {
          x: (i) => fanFrom[i].x,
          y: (i) => fanFrom[i].y,
          rotation: (i) => fanFrom[i].rotation,
          opacity: 1
        },
        {
          x: (i) => fanTo[i].x,
          y: (i) => fanTo[i].y,
          rotation: (i) => fanTo[i].rotation,
          ease: "none",
          scrollTrigger: {
            trigger: ".hero",
            start: "top 80%",
            end: "center 30%",
            scrub: 0.6,
            invalidateOnRefresh: true
          }
        }
      );
    }
  }

  const frame = document.getElementById("video-frame");
  function screenScale() {
    const baseW = frame.offsetWidth;
    const baseH = frame.offsetHeight;
    if (!baseW || !baseH) return 1;
    return Math.min(window.innerWidth / baseW, window.innerHeight / baseH);
  }
  if (frame && !reduce) {
    gsap.fromTo(frame,
      {
        scale: () => screenScale() * 0.58,
        borderRadius: 16
      },
      {
        scale: () => screenScale(),
        borderRadius: 12,
        ease: "none",
        scrollTrigger: {
          trigger: ".video-section",
          start: "top 88%",
          end: "bottom 42%",
          scrub: true,
          invalidateOnRefresh: true
        }
      }
    );
    window.addEventListener("resize", () => ScrollTrigger.refresh());
  } else if (frame) {
    gsap.set(frame, { scale: screenScale(), borderRadius: 12 });
  }

  document.querySelectorAll(".ring").forEach((ring) => {
    const value = ring.querySelector(".ring-value");
    const figure = ring.querySelector(".ring-num");
    if (!value || !figure) return;
    const percent = Number(ring.dataset.percent) || 0;
    const count = Number(ring.dataset.count) || 0;
    const decimals = Number(ring.dataset.decimals) || 0;
    const suffix = ring.dataset.suffix || "";
    const length = 2 * Math.PI * 64;
    value.style.strokeDasharray = String(length);
    const end = length * (1 - Math.min(percent, 100) / 100);
    const counter = { n: 0 };
    const paint = () => {
      figure.textContent = counter.n.toFixed(decimals) + suffix;
    };
    if (reduce) {
      value.style.strokeDashoffset = String(end);
      counter.n = count;
      paint();
      return;
    }
    value.style.strokeDashoffset = String(length);
    const tl = gsap.timeline({
      scrollTrigger: { trigger: ring, start: "top 82%", once: true }
    });
    tl.to(value, { strokeDashoffset: end, duration: 1.4, ease: "power2.out" }, 0);
    tl.to(counter, {
      n: count,
      duration: 1.4,
      ease: "power2.out",
      onUpdate: paint
    }, 0);
  });

  const viewport = document.getElementById("benefit-viewport");
  const track = document.getElementById("benefit-track");
  const hint = document.getElementById("drag-hint");
  if (viewport && track && window.Draggable) {
    const slides = track.querySelectorAll(".benefit-card");
    const step = () => {
      const card = slides[0];
      if (!card) return 1;
      const gap = 20;
      return card.getBoundingClientRect().width + gap;
    };
    const maxScroll = () => -Math.max(0, track.scrollWidth - viewport.clientWidth);

    let velocity = 0;
    let lastX = 0;
    let lastT = 0;

    const draggable = Draggable.create(track, {
      type: "x",
      inertia: hasInertia,
      bounds: viewport,
      cursor: "grab",
      activeCursor: "grabbing",
      snap: hasInertia
        ? (value) => gsap.utils.clamp(maxScroll(), 0, Math.round(value / step()) * step())
        : false,
      onPress() {
        viewport.classList.add("is-dragging");
        lastX = this.x;
        lastT = performance.now();
        velocity = 0;
      },
      onDrag() {
        const now = performance.now();
        const dt = Math.max(16, now - lastT);
        velocity = (this.x - lastX) / dt;
        lastX = this.x;
        lastT = now;
      },
      onRelease() {
        viewport.classList.remove("is-dragging");
        if (hasInertia) return;
        const size = step();
        const projected = this.x + velocity * 180;
        const snapped = gsap.utils.clamp(maxScroll(), 0, Math.round(projected / size) * size);
        gsap.to(track, { x: snapped, duration: 0.55, ease: "power2.out" });
      }
    })[0];

    function go(direction) {
      const current = Number(gsap.getProperty(track, "x")) || 0;
      const next = gsap.utils.clamp(maxScroll(), 0, current + direction * step());
      gsap.to(track, { x: next, duration: 0.55, ease: "power2.out" });
    }
    document.getElementById("benefit-prev")?.addEventListener("click", () => go(1));
    document.getElementById("benefit-next")?.addEventListener("click", () => go(-1));
    window.addEventListener("resize", () => draggable?.applyBounds(viewport));

    if (hint && window.matchMedia("(hover: hover)").matches) {
      document.body.appendChild(hint);
      const place = (event) => {
        hint.style.transform = "translate(" + event.clientX + "px, " + event.clientY + "px) translate(-50%, -50%)";
      };
      viewport.addEventListener("pointerenter", (event) => {
        hint.classList.add("is-on");
        viewport.classList.add("is-hint");
        place(event);
      });
      viewport.addEventListener("pointerleave", () => {
        hint.classList.remove("is-on");
        viewport.classList.remove("is-hint");
      });
      viewport.addEventListener("pointermove", place);
    }
  }

  window.addEventListener("load", () => ScrollTrigger.refresh());
})();
